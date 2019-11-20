using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mmm.Platform.IoT.IdentityGateway.Services;
using Mmm.Platform.IoT.IdentityGateway.Services.Helpers;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Mmm.Platform.IoT.IdentityGateway.Services.Test.Helpers.Builders;
using TestStack.Dossier;
using TestStack.Dossier.Lists;
using Xunit;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Test
{
    public class UserTenantContainerTest
    {
        private UserTenantContainer userTenantContainer;
        private Mock<ITableHelper> mockTableHelper;
        private const int dynamicTableEntityCount = 100;
        private Random random = new Random();
        private UserTenantInput someUserTenantInput = Builder<UserTenantInput>.CreateNew().Build();
        private IList<DynamicTableEntity> dynamicTableEntities;
        private AnonymousValueFixture any = new AnonymousValueFixture();

        public UserTenantContainerTest()
        {
            InitializeSystemUnderTest();
            SetupDefaultBehaviors();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetAllReturnsExpectedUserTenantList()
        {
            // Arrange
            dynamicTableEntities = DynamicTableEntityBuilder
                .CreateListOfSize(dynamicTableEntityCount)
                .All()
                .WithRandomRolesProperty()
                .TheLast(random.Next(0, dynamicTableEntityCount))
                .Set(dte => dte.PartitionKey, someUserTenantInput.UserId)
                .BuildList();

            // Act
            var result = await userTenantContainer.GetAllAsync(someUserTenantInput);

            // Assert
            Assert.Equal("gettenants", result.batchMethod.ToLowerInvariant());
            Assert.NotNull(result.models);
            Assert.Equal(dynamicTableEntities.Count(dte => dte.PartitionKey == someUserTenantInput.UserId), result.models.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetAllUsersReturnsExpectedUserTenantList()
        {
            // Arrange
            dynamicTableEntities = DynamicTableEntityBuilder
                .CreateListOfSize(dynamicTableEntityCount)
                .All()
                .WithRandomRolesProperty()
                .TheLast(random.Next(0, dynamicTableEntityCount))
                .Set(dte => dte.RowKey, someUserTenantInput.Tenant)
                .BuildList();

            // Act
            var result = await userTenantContainer.GetAllAsync(someUserTenantInput);

            // Assert
            Assert.Equal("gettenants", result.batchMethod.ToLowerInvariant());
            Assert.NotNull(result.models);
            Assert.Equal(dynamicTableEntities.Count(dte => dte.PartitionKey == someUserTenantInput.UserId), result.models.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetAllReturnsEmptyUserTenantList()
        {
            // Arrange
            dynamicTableEntities = new List<DynamicTableEntity>();

            // Act
            var result = await userTenantContainer.GetAllAsync(someUserTenantInput);

            // Assert
            Assert.Equal("gettenants", result.batchMethod.ToLowerInvariant());
            Assert.NotNull(result.models);
            Assert.Empty(result.models);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetReturnsExpectedUserTenant()
        {
            // Arrange
            dynamicTableEntities = DynamicTableEntityBuilder
                .CreateListOfSize(dynamicTableEntityCount)
                .All()
                .WithRandomRolesProperty()
                .TheLast(1)
                .Set(dte => dte.PartitionKey, someUserTenantInput.UserId)
                .Set(dte => dte.RowKey, someUserTenantInput.Tenant)
                .BuildList();

            // Act
            var result = await userTenantContainer.GetAsync(someUserTenantInput);

            // Assert
            AssertUserTenantMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void GetReturnsEmptyUserTenant()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();

            // Act
            var result = await userTenantContainer.GetAsync(someUserTenantInput);

            // Assert
            Assert.Null(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateReturnsExpectedUserTenant()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();

            // Act
            var result = await userTenantContainer.CreateAsync(someUserTenantInput);

            // Assert
            AssertUserTenantMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateHandlesNullUserIdAndReturnsExpectedUserTenant()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();
            someUserTenantInput.UserId = null;

            // Act
            var result = await userTenantContainer.CreateAsync(someUserTenantInput);

            // Assert
            AssertUserTenantMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateThrowsWhenUserTenantAlreadyExist()
        {
            // Arrange
            SetupSuccessfulGetOnTableHelper();

            // Act
            Func<Task> a = async () => await userTenantContainer.CreateAsync(someUserTenantInput);

            // Assert
            await Assert.ThrowsAsync<StorageException>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void UpdateReturnsExpectedUserTenant()
        {
            // Arrange
            someUserTenantInput = Builder<UserTenantInput>.CreateNew().Set(uti => uti.Roles, JsonConvert.SerializeObject(new[] { "someRole", "someOtherRole" })).Build();

            // Act
            var result = await userTenantContainer.UpdateAsync(someUserTenantInput);

            // Assert
            AssertUserTenantMatchesInput(result);
        }

        public static IEnumerable<object[]> GetRoleLists()
        {
            yield return new object[] { null };
            yield return new object[] { string.Empty };
            yield return new object[] { " " };
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [MemberData(nameof(GetRoleLists))]
        public async void UpdateDoesNotThrowWhenUserTenantRoleListIsNullOrEmptyOrWhitespace(string roles)
        {
            // Arrange
            someUserTenantInput.Roles = roles;

            // Act
            // Assert
            await userTenantContainer.UpdateAsync(someUserTenantInput);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteReturnsExpectedUserTenant()
        {
            // Arrange
            SetupSuccessfulGetOnTableHelper();

            // Act
            var result = await userTenantContainer.DeleteAsync(someUserTenantInput);

            // Assert
            AssertUserTenantMatchesInput(result);
        }
        
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteThrowsWhenUserTenantDoesNotExist()
        {
            // Arrange
            SetupEmptyGetOnTableHelper();

            // Act
            Func<Task> a = async () => await userTenantContainer.DeleteAsync(someUserTenantInput);

            // Assert
            await Assert.ThrowsAsync<StorageException>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteAllReturnsExpectedUserTenantList()
        {
            // Arrange
            dynamicTableEntities = DynamicTableEntityBuilder
                .CreateListOfSize(dynamicTableEntityCount)
                .All()
                .WithRandomRolesProperty()
                .TheLast(random.Next(0, dynamicTableEntityCount))
                .Set(dte => dte.RowKey, someUserTenantInput.Tenant)
                .BuildList();

            // Act
            var result = await userTenantContainer.DeleteAllAsync(someUserTenantInput);
            
            // Assert
            Assert.Equal("delete", result.batchMethod.ToLowerInvariant());
            Assert.NotNull(result.models);
            Assert.Equal(dynamicTableEntities.Count(dte => dte.PartitionKey == someUserTenantInput.UserId), result.models.Count);
        }

        private void InitializeSystemUnderTest()
        {
            mockTableHelper = new Mock<ITableHelper> { DefaultValue = DefaultValue.Mock };
            userTenantContainer = new UserTenantContainer(mockTableHelper.Object);
        }

        private void SetupDefaultBehaviors()
        {
            mockTableHelper.Setup(m => m.QueryAsync(userTenantContainer.TableName, It.IsAny<TableQuery>(), null))
                .ReturnsAsync((string tableName, TableQuery query, TableContinuationToken token) => QueryAsync(tableName, query, token));
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userTenantContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Retrieve)))
                .ReturnsAsync((string tableName, TableOperation operation) => ExecuteRetrieveOperationAsync(tableName, operation));
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userTenantContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Insert || to.OperationType == TableOperationType.Replace || to.OperationType == TableOperationType.InsertOrReplace || to.OperationType == TableOperationType.Delete || to.OperationType == TableOperationType.InsertOrMerge || to.OperationType == TableOperationType.Merge)))
                .ReturnsAsync((string tableName, TableOperation operation) => ExecuteInsertOrReplaceOrDeleteOrMergeOperationAsync(tableName, operation));
        }

        private TableQuerySegment QueryAsync(string tableName, TableQuery query, TableContinuationToken token)
        {
            var matchingDynamicTableEntities = dynamicTableEntities.Where(dte => dte.PartitionKey == someUserTenantInput.UserId).ToList();
            var tableQuerySegment = TypeHelpers.CreateInstance<TableQuerySegment>(matchingDynamicTableEntities);
            return tableQuerySegment;
        }

        private TableResult ExecuteRetrieveOperationAsync(string tableName, TableOperation operation)
        {
            var operationPartitionKey = operation.GetPropertyValue("PartitionKey") as string;
            var operationRowKey = operation.GetPropertyValue("RowKey") as string;
            return new TableResult { Result = new UserTenantModel(dynamicTableEntities.FirstOrDefault(dte => dte.PartitionKey == operationPartitionKey && dte.RowKey == operationRowKey)) };
        }

        private TableResult ExecuteInsertOrReplaceOrDeleteOrMergeOperationAsync(string tableName, TableOperation operation)
        {
            return new TableResult { Result = new UserTenantModel(operation.Entity.PartitionKey, operation.Entity.RowKey, (operation.Entity as UserTenantModel).Roles) };
        }

        private void SetupEmptyGetOnTableHelper()
        {
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userTenantContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Retrieve))).ReturnsAsync(new TableResult { Result = null });
        }

        private void SetupSuccessfulGetOnTableHelper()
        {
            mockTableHelper.Setup(m => m.ExecuteOperationAsync(userTenantContainer.TableName, It.Is<TableOperation>(to => to.OperationType == TableOperationType.Retrieve))).ReturnsAsync(new TableResult { Result = new UserTenantModel() });
        }

        private void AssertUserTenantMatchesInput(UserTenantModel userTenant)
        {
            Assert.NotNull(userTenant);
            userTenant = new UserTenantModel(someUserTenantInput);
            Assert.Equal(userTenant.Name, userTenant.Name);
            Assert.Equal(userTenant.RoleList, userTenant.RoleList);
            Assert.Equal(userTenant.Roles, userTenant.Roles);
            Assert.Equal(userTenant.TenantId, userTenant.TenantId);
            Assert.Equal(userTenant.Type, userTenant.Type);
            Assert.Equal(userTenant.UserId, userTenant.UserId);
        }
    }
}
