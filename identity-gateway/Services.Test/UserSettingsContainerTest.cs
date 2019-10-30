using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityGateway.Services;
using IdentityGateway.Services.Helpers;
using IdentityGateway.Services.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Services.Test.Helpers;
using Services.Test.Helpers.Builders;
using TestStack.Dossier;
using TestStack.Dossier.Lists;
using Xunit;

namespace Services.Test
{
    public class UserSettingsContainerTest
    {
        private UserSettingsContainer userSettingsContainer;
        private Mock<ITableHelper> mockTableHelper;
        private const int dynamicTableEntityCount = 100;
        private Random random = new Random();
        private UserSettingsInput someUserSettingsInput = Builder<UserSettingsInput>.CreateNew().Build();
        private IList<DynamicTableEntity> dynamicTableEntities;

        public UserSettingsContainerTest()
        {
            InitializeSystemUnderTest();
            SetupDefaultBehaviors();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetAllReturnsExpectedUserSettingsList()
        {
            // Arrange
            dynamicTableEntities = DynamicTableEntityBuilder
                .CreateListOfSize(dynamicTableEntityCount)
                .All()
                .WithRandomValueProperty()
                .TheLast(random.Next(0, dynamicTableEntityCount))
                .Set(dte => dte.PartitionKey, someUserSettingsInput.UserId)
                .BuildList();

            // Act
            var result = await userSettingsContainer.GetAllAsync(someUserSettingsInput);

            // Assert
            Assert.Equal("get", result.batchMethod.ToLowerInvariant());
            Assert.NotNull(result.models);
            Assert.Equal(dynamicTableEntities.Count(dte => dte.PartitionKey == someUserSettingsInput.UserId), result.models.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetAllReturnsEmptyUserSettingsList()
        {
            // Arrange
            dynamicTableEntities = new List<DynamicTableEntity>();

            // Act
            var result = await userSettingsContainer.GetAllAsync(someUserSettingsInput);

            // Assert
            Assert.Equal("get", result.batchMethod.ToLowerInvariant());
            Assert.NotNull(result.models);
            Assert.Empty(result.models);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetReturnsExpectedUserSettings()
        {
            // Arrange
            dynamicTableEntities = DynamicTableEntityBuilder
                .CreateListOfSize(dynamicTableEntityCount)
                .All()
                .WithRandomValueProperty()
                .TheLast(1)
                .Set(dte => dte.PartitionKey, someUserSettingsInput.UserId)
                .Set(dte => dte.RowKey, someUserSettingsInput.SettingKey)
                .BuildList();

            // Act
            var result = await userSettingsContainer.GetAsync(someUserSettingsInput);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        private void AssertUserSettingsMatchesInput(UserSettingsModel userSettings)
        {
            Assert.NotNull(userSettings);
            userSettings = new UserSettingsModel(someUserSettingsInput);
            Assert.Equal(userSettings.SettingKey, userSettings.SettingKey);
            Assert.Equal(userSettings.UserId, userSettings.UserId);
            Assert.Equal(userSettings.Value, userSettings.Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetReturnsEmptyUserSettings()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();

            // Act
            var result = await userSettingsContainer.GetAsync(someUserSettingsInput);

            // Assert
            Assert.Null(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateReturnsExpectedUserSettings()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();

            // Act
            var result = await userSettingsContainer.CreateAsync(someUserSettingsInput);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateThrowsWhenUserSettingsAlreadyExist()
        {
            // Arrange
            SetupSuccessfulGetOnTableHelper();

            // Act
            Func<Task> a = async () => await userSettingsContainer.CreateAsync(someUserSettingsInput);

            // Assert
            await Assert.ThrowsAsync<StorageException>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void UpdateReturnsExpectedUserSettings()
        {
            // Arrange
            // Act
            var result = await userSettingsContainer.UpdateAsync(someUserSettingsInput);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteReturnsExpectedUserSettings()
        {
            // Arrange
            SetupSuccessfulGetOnTableHelper();

            // Act
            var result = await userSettingsContainer.DeleteAsync(someUserSettingsInput);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteThrowsWhenUserSettingsDoesNotExist()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();

            // Act
            Func<Task> a = async () => await userSettingsContainer.DeleteAsync(someUserSettingsInput);

            // Assert
            await Assert.ThrowsAsync<StorageException>(a);
        }

        private void InitializeSystemUnderTest()
        {
            mockTableHelper = new Mock<ITableHelper> { DefaultValue = DefaultValue.Mock };
            userSettingsContainer = new UserSettingsContainer(mockTableHelper.Object);
        }

        private void SetupDefaultBehaviors()
        {
            mockTableHelper.Setup(m => m.QueryAsync(userSettingsContainer.TableName, It.IsAny<TableQuery>(), null))
                .ReturnsAsync((string tableName, TableQuery query, TableContinuationToken token) => QueryAsync(tableName, query, token));
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userSettingsContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Retrieve)))
                .ReturnsAsync((string tableName, TableOperation operation) => ExecuteRetrieveOperationAsync(tableName, operation));
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userSettingsContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Insert || to.OperationType == TableOperationType.Replace || to.OperationType == TableOperationType.InsertOrReplace || to.OperationType == TableOperationType.Delete)))
                .ReturnsAsync((string tableName, TableOperation operation) => ExecuteInsertOrReplaceOrDeleteOperationAsync(tableName, operation));
        }

        private TableQuerySegment QueryAsync(string tableName, TableQuery query, TableContinuationToken token)
        {
            var matchingDynamicTableEntities = dynamicTableEntities.Where(dte => dte.PartitionKey == someUserSettingsInput.UserId).ToList();
            var tableQuerySegment = TypeHelpers.CreateInstance<TableQuerySegment>(matchingDynamicTableEntities);
            return tableQuerySegment;
        }

        private TableResult ExecuteRetrieveOperationAsync(string tableName, TableOperation operation)
        {
            var operationPartitionKey = operation.GetPropertyValue("PartitionKey") as string;
            var operationRowKey = operation.GetPropertyValue("RowKey") as string;
            return new TableResult { Result = new UserSettingsModel(dynamicTableEntities.FirstOrDefault(dte => dte.PartitionKey == operationPartitionKey && dte.RowKey == operationRowKey)) };
        }

        private TableResult ExecuteInsertOrReplaceOrDeleteOperationAsync(string tableName, TableOperation operation)
        {
            return new TableResult { Result = new UserSettingsModel(operation.Entity.PartitionKey, operation.Entity.RowKey, (operation.Entity as UserSettingsModel).Value) };
        }

        private void SetupEmptyGetOnTableHelper()
        {
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userSettingsContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Retrieve))).ReturnsAsync(new TableResult { Result = null });
        }

        private void SetupSuccessfulGetOnTableHelper()
        {
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userSettingsContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Retrieve))).ReturnsAsync(new TableResult { Result = new UserSettingsModel() });
        }
    }
}
