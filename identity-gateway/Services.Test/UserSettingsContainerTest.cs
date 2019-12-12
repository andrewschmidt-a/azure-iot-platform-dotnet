using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Mmm.Platform.IoT.IdentityGateway.Services.Models;
using Mmm.Platform.IoT.IdentityGateway.Services.Test.Helpers.Builders;
using Mmm.Platform.IoT.Common.Services.External.TableStorage;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using TestStack.Dossier;
using TestStack.Dossier.Lists;
using Xunit;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Test
{
    public class UserSettingsContainerTest
    {
        private UserSettingsContainer userSettingsContainer;
        private Mock<ITableStorageClient> mockTableStorageClient;
        private const int dynamicTableEntityCount = 100;
        private Random random = new Random();
        private UserSettingsInput someUserSettingsInput = Builder<UserSettingsInput>.CreateNew().Build();
        private IList<DynamicTableEntity> dynamicTableEntities;

        public UserSettingsContainerTest()
        {
            this.mockTableStorageClient = new Mock<ITableStorageClient> { DefaultValue = DefaultValue.Mock };
            this.userSettingsContainer = new UserSettingsContainer(mockTableStorageClient.Object);
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

            mockTableStorageClient
                .Setup(m => m.QueryAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<TableQuery<UserSettingsModel>>(),
                    It.Is<CancellationToken>(t => t == default(CancellationToken))))
                .ReturnsAsync(dynamicTableEntities
                    .Where(dte => dte.PartitionKey == someUserSettingsInput.UserId)
                    .Select(e => new UserSettingsModel(e))
                    .ToList());

            // Act
            var result = await userSettingsContainer.GetAllAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.QueryAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<TableQuery<UserSettingsModel>>(),
                    It.Is<CancellationToken>(t => t == default(CancellationToken))),
                Times.Once);

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

            mockTableStorageClient
                .Setup(m => m.QueryAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<TableQuery<UserSettingsModel>>(),
                    It.Is<CancellationToken>(t => t == default(CancellationToken))))
                .ReturnsAsync(dynamicTableEntities
                    .Where(dte => dte.PartitionKey == someUserSettingsInput.UserId)
                    .Select(e => new UserSettingsModel(e))
                    .ToList());

            // Act
            var result = await userSettingsContainer.GetAllAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.QueryAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<TableQuery<UserSettingsModel>>(),
                    It.Is<CancellationToken>(t => t == default(CancellationToken))),
                Times.Once);

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

            mockTableStorageClient
                .Setup(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .ReturnsAsync((string tableName, string partitionKey, string rowKey) => 
                {
                    return new UserSettingsModel(
                        dynamicTableEntities.FirstOrDefault(dte => 
                        {
                            return dte.PartitionKey == partitionKey && dte.RowKey == rowKey;
                        }));
                });

            // Act
            var result = await userSettingsContainer.GetAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.RetrieveAsync<ITableEntity>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()),
                Times.Once);

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
            mockTableStorageClient
                .Setup(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .ReturnsAsync((UserSettingsModel)null);

            // Act
            var result = await userSettingsContainer.GetAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()),
                Times.Once);

            // Assert
            Assert.Null(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateReturnsExpectedUserSettings()
        {
            // mock intial check to see if setting already exists during CreateAsync
            mockTableStorageClient
                .Setup(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .ReturnsAsync((UserSettingsModel)null);
            
            // mock call to insert new setting
            mockTableStorageClient
                .Setup(m => m.InsertAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.Is<UserSettingsModel>(u => u.PartitionKey == someUserSettingsInput.UserId && u.RowKey == someUserSettingsInput.SettingKey && u.Value == someUserSettingsInput.Value)))
                .ReturnsAsync(new UserSettingsModel(someUserSettingsInput));

            // Act
            var result = await userSettingsContainer.CreateAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()),
                Times.Once);

            mockTableStorageClient
                .Verify(m => m.InsertAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.Is<UserSettingsModel>(u => u.PartitionKey == someUserSettingsInput.UserId && u.RowKey == someUserSettingsInput.SettingKey && u.Value == someUserSettingsInput.Value)),
                Times.Once);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void CreateThrowsWhenUserSettingsAlreadyExist()
        {
            // Arrange
            // mock the initial check to see if setting already exists
            // no need to mock the Insert call as the exception should be thrown before it is invoked
            mockTableStorageClient
                .Setup(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .ReturnsAsync(new UserSettingsModel());

            // Act
            Func<Task> a = async () => await userSettingsContainer.CreateAsync(someUserSettingsInput);

            // Assert
            await Assert.ThrowsAsync<StorageException>(a);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void UpdateReturnsExpectedUserSettings()
        {
            // Arrange
            mockTableStorageClient
                .Setup(m => m.InsertOrReplaceAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.Is<UserSettingsModel>(u => u.PartitionKey == someUserSettingsInput.UserId && u.RowKey == someUserSettingsInput.SettingKey && u.Value == someUserSettingsInput.Value)))
                .ReturnsAsync(new UserSettingsModel(someUserSettingsInput));

            // Act
            var result = await userSettingsContainer.UpdateAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.InsertOrReplaceAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.Is<UserSettingsModel>(u => u.PartitionKey == someUserSettingsInput.UserId && u.RowKey == someUserSettingsInput.SettingKey && u.Value == someUserSettingsInput.Value)),
                Times.Once);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteReturnsExpectedUserSettings()
        {
            // Arrange
            mockTableStorageClient
                .Setup(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .ReturnsAsync(new UserSettingsModel(someUserSettingsInput));
            
            mockTableStorageClient
                .Setup(m => m.DeleteAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.Is<UserSettingsModel>(u => u.PartitionKey == someUserSettingsInput.UserId && u.RowKey == someUserSettingsInput.SettingKey && u.Value == someUserSettingsInput.Value)))
                .ReturnsAsync(new UserSettingsModel(someUserSettingsInput));

            // Act
            var result = await userSettingsContainer.DeleteAsync(someUserSettingsInput);

            mockTableStorageClient
                .Verify(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()),
                Times.Once);
        
            mockTableStorageClient
                .Verify(m => m.DeleteAsync(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.Is<UserSettingsModel>(u => u.PartitionKey == someUserSettingsInput.UserId && u.RowKey == someUserSettingsInput.SettingKey && u.Value == someUserSettingsInput.Value)),
                Times.Once);

            // Assert
            AssertUserSettingsMatchesInput(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async void DeleteThrowsWhenUserSettingsDoesNotExist()
        {
            // Arrange
            mockTableStorageClient
                .Setup(m => m.RetrieveAsync<UserSettingsModel>(
                    It.Is<String>(n => n == userSettingsContainer.TableName),
                    It.IsAny<String>(),
                    It.IsAny<String>()))
                .ReturnsAsync((UserSettingsModel)null);

            // Act
            Func<Task> a = async () => await userSettingsContainer.DeleteAsync(someUserSettingsInput);

            // Assert
            await Assert.ThrowsAsync<StorageException>(a);
        }
    }
}
