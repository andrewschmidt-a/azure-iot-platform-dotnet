using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Mmm.Platform.IoT.Common.TestHelpers;
using Mmm.Platform.IoT.IoTHubManager.Services.Helpers;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Test
{
    public class DeploymentsTest
    {
        private const string TEST_EDGE_PACKAGE_JSON =
                @"{
                    ""id"": ""tempid"",
                    ""schemaVersion"": ""1.0"",
                    ""content"": {
                        ""modulesContent"": {
                        ""$edgeAgent"": {
                            ""properties.desired"": {
                            ""schemaVersion"": ""1.0"",
                            ""runtime"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                ""loggingOptions"": """",
                                ""minDockerVersion"": ""v1.25""
                                }
                            },
                            ""systemModules"": {
                                ""edgeAgent"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                    ""image"": ""mcr.microsoft.com/azureiotedge-agent:1.0"",
                                    ""createOptions"": ""{}""
                                }
                                },
                                ""edgeHub"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                    ""image"": ""mcr.microsoft.com/azureiotedge-hub:1.0"",
                                    ""createOptions"": ""{}""
                                },
                                ""status"": ""running"",
                                ""restartPolicy"": ""always""
                                }
                            },
                            ""modules"": {}
                            }
                        },
                        ""$edgeHub"": {
                            ""properties.desired"": {
                            ""schemaVersion"": ""1.0"",
                            ""routes"": {
                                ""route"": ""FROM /messages/* INTO $upstream""
                            },
                            ""storeAndForwardConfiguration"": {
                                ""timeToLiveSecs"": 7200
                            }
                            }
                        }
                        }
                    },
                    ""targetCondition"": ""*"",
                    ""priority"": 30,
                    ""labels"": {
                        ""Name"": ""Test""
                    },
                    ""createdTimeUtc"": ""2018-08-20T18:05:55.482Z"",
                    ""lastUpdatedTimeUtc"": ""2018-08-20T18:05:55.482Z"",
                    ""etag"": null,
                    ""metrics"": {
                        ""results"": {},
                        ""queries"": {}
                    }
                 }";

        private const string TEST_ADM_PACKAGE_JSON =
                @"{
                    ""id"": ""tempid"",
                    ""schemaVersion"": ""1.0"",
                    ""content"": {
                        ""modulesContent"": {
                        ""$edgeAgent"": {
                            ""properties.desired"": {
                            ""schemaVersion"": ""1.0"",
                            ""runtime"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                ""loggingOptions"": """",
                                ""minDockerVersion"": ""v1.25""
                                }
                            },
                            ""systemModules"": {
                                ""edgeAgent"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                    ""image"": ""mcr.microsoft.com/azureiotedge-agent:1.0"",
                                    ""createOptions"": ""{}""
                                }
                                },
                                ""edgeHub"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                    ""image"": ""mcr.microsoft.com/azureiotedge-hub:1.0"",
                                    ""createOptions"": ""{}""
                                },
                                ""status"": ""running"",
                                ""restartPolicy"": ""always""
                                }
                            },
                            ""modules"": {}
                            }
                        },
                        ""$edgeHub"": {
                            ""properties.desired"": {
                            ""schemaVersion"": ""1.0"",
                            ""routes"": {
                                ""route"": ""FROM /messages/* INTO $upstream""
                            },
                            ""storeAndForwardConfiguration"": {
                                ""timeToLiveSecs"": 7200
                            }
                            }
                        }
                        }
                    },
                    ""targetCondition"": ""*"",
                    ""priority"": 30,
                    ""labels"": {
                        ""Name"": ""Test""
                    },
                    ""createdTimeUtc"": ""2018-08-20T18:05:55.482Z"",
                    ""lastUpdatedTimeUtc"": ""2018-08-20T18:05:55.482Z"",
                    ""etag"": null,
                    ""metrics"": {
                        ""results"": {},
                        ""queries"": {}
                    }mockIoTHub
                 }";
        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_GROUP_NAME_LABEL = "DeviceGroupName";
        private const string CONFIG_TYPE_LABEL = "ConfigType";
        private const string RM_CREATED_LABEL = "RMDeployment";
        private const string RESOURCE_NOT_FOUND_EXCEPTION =
            "Mmm.Platform.IoT.Common.Services.Exceptions.ResourceNotSupportedException, Mmm.Platform.IoT.Common.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string DEPLOYMENT_PACKAGE_NAME_LABEL = "PackageName";
        private readonly Deployments deployments;
        private readonly Mock<RegistryManager> registry;
        private readonly string ioTHubHostName = "mockIoTHub";
        private Mock<ITenantConnectionHelper> tenantHelper;
        private string packageTypeLabel = "Type";

        public DeploymentsTest()
        {
            this.registry = new Mock<RegistryManager>();
            tenantHelper = new Mock<ITenantConnectionHelper>();
            tenantHelper.Setup(e => e.GetIotHubName()).Returns(this.ioTHubHostName);
            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            MockIdentity.MockClaims("one");
            this.deployments = new Deployments(tenantHelper.Object);
        }

        [Theory]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("depname", "dvcgroupid", "dvcquery", TEST_EDGE_PACKAGE_JSON, 10, "")]
        [InlineData("", "dvcgroupid", "dvcquery", TEST_EDGE_PACKAGE_JSON, 10, "System.ArgumentNullException")]
        [InlineData("depname", "", "dvcquery", TEST_EDGE_PACKAGE_JSON, 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "", TEST_EDGE_PACKAGE_JSON, 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "dvcquery", "", 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "dvcquery", TEST_EDGE_PACKAGE_JSON, -1, "System.ArgumentOutOfRangeException")]
        public async Task CreateDeploymentTest(
            string deploymentName,
            string deviceGroupId,
            string deviceGroupQuery,
            string packageContent,
            int priority,
            string expectedException)
        {
            // Arrange
            var depModel = new DeploymentServiceModel()
            {
                Name = deploymentName,
                DeviceGroupId = deviceGroupId,
                DeviceGroupQuery = deviceGroupQuery,
                PackageContent = packageContent,
                PackageType = PackageType.EdgeManifest,
                Priority = priority
            };

            var newConfig = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, deploymentName },
                    { packageTypeLabel, PackageType.EdgeManifest.ToString() },
                    { DEPLOYMENT_GROUP_ID_LABEL, deviceGroupId },
                    { RM_CREATED_LABEL, bool.TrueString },
                },
                Priority = priority
            };

            this.registry.Setup(r => r.AddConfigurationAsync(It.Is<Configuration>(c =>
                    c.Labels.ContainsKey(DEPLOYMENT_NAME_LABEL) &&
                    c.Labels.ContainsKey(DEPLOYMENT_GROUP_ID_LABEL) &&
                    c.Labels.ContainsKey(RM_CREATED_LABEL) &&
                    c.Labels[DEPLOYMENT_NAME_LABEL] == deploymentName &&
                    c.Labels[DEPLOYMENT_GROUP_ID_LABEL] == deviceGroupId &&
                    c.Labels[RM_CREATED_LABEL] == bool.TrueString)))
                .ReturnsAsync(newConfig);

            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            // Act
            if (string.IsNullOrEmpty(expectedException))
            {
                var createdDeployment = await this.deployments.CreateAsync(depModel);

                // Assert
                Assert.False(string.IsNullOrEmpty(createdDeployment.Id));
                Assert.Equal(deploymentName, createdDeployment.Name);
                Assert.Equal(deviceGroupId, createdDeployment.DeviceGroupId);
                Assert.Equal(priority, createdDeployment.Priority);
            }
            else
            {
                await Assert.ThrowsAsync(
                    Type.GetType(expectedException),
                    async () => await this.deployments.CreateAsync(depModel));
            }
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task InvalidRmConfigurationTest()
        {
            // Arrange
            var configuration = this.CreateConfiguration(0, false);

            this.registry.Setup(r => r.GetConfigurationAsync(It.IsAny<string>()))
                .ReturnsAsync(configuration);
            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            // Act & Assert
            await Assert.ThrowsAsync(
                Type.GetType(RESOURCE_NOT_FOUND_EXCEPTION),
                async () => await this.deployments.GetAsync(configuration.Id));
        }

        [Theory]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetDeploymentsTest(int numDeployments)
        {
            // Arrange
            var configurations = new List<Configuration>();
            for (int i = numDeployments - 1; i >= 0; i--)
            {
                configurations.Add(this.CreateConfiguration(i, true));
            }

            this.registry.Setup(r => r.GetConfigurationsAsync(20)).ReturnsAsync(configurations);
            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            // Act
            var returnedDeployments = await this.deployments.ListAsync();

            // Assert
            Assert.Equal(numDeployments, returnedDeployments.Items.Count);

            // verify deployments are ordered by name
            for (int i = 0; i < numDeployments; i++)
            {
                Assert.Equal("deployment" + i, returnedDeployments.Items[i].Name);
            }
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetDeploymentsWithDeviceStatusTest()
        {
            // Arrange
            var configuration = this.CreateConfiguration(0, true);
            var deploymentId = configuration.Id;
            this.registry.Setup(r => r.GetConfigurationAsync(deploymentId)).ReturnsAsync(configuration);

            IQuery queryResult = new ResultQuery(3);
            this.registry.Setup(r => r.CreateQuery(It.IsAny<string>())).Returns(queryResult);

            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);
            // Act
            var returnedDeployment = await this.deployments.GetAsync(deploymentId, true);
            var deviceStatuses = returnedDeployment.DeploymentMetrics.DeviceStatuses;
            Assert.Equal(3, deviceStatuses.Count);

            // Assert
            returnedDeployment = await this.deployments.GetAsync(deploymentId);
            deviceStatuses = returnedDeployment.DeploymentMetrics.DeviceStatuses;
            Assert.Null(deviceStatuses);
        }

        [Theory]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(true, true, true)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false)]
        [InlineData(true, false, true)]
        public async Task GetDeploymentTypeTest(bool isEdgeContent, bool addLabel, bool isEdgeLabel = false)
        {
            // Arrange
            var content = new ConfigurationContent()
            {
                ModulesContent = isEdgeContent ? new Dictionary<string, IDictionary<string, object>>() : null,
                DeviceContent = !isEdgeContent ? new Dictionary<string, object>() : null
            };

            var label = string.Empty;

            if (addLabel)
            {
                label = isEdgeLabel ? PackageType.EdgeManifest.ToString() :
                    PackageType.DeviceConfiguration.ToString();
            }

            var configuration = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, string.Empty },
                    { DEPLOYMENT_GROUP_ID_LABEL, string.Empty },
                    { packageTypeLabel, label },
                    { CONFIG_TYPE_LABEL, "CustomConfig" },
                    { RM_CREATED_LABEL, bool.TrueString },
                },
                Content = content
            };

            var deploymentId = configuration.Id;
            this.registry.Setup(r => r.GetConfigurationAsync(deploymentId)).ReturnsAsync(configuration);
            this.registry.Setup(r => r.CreateQuery(It.IsAny<string>())).Returns(new ResultQuery(0));

            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);
            // Act
            var returnedDeployment = await this.deployments.GetAsync(deploymentId);

            // Assert Should returned Deplyment PackageType according to label
            if (addLabel)
            {
                if (isEdgeLabel)
                {
                    Assert.Equal(PackageType.EdgeManifest, returnedDeployment.PackageType);
                }
                else
                {
                    Assert.Equal(PackageType.DeviceConfiguration, returnedDeployment.PackageType);
                }
            }
            else
            {
                if (isEdgeContent)
                {
                    Assert.Equal(PackageType.EdgeManifest, returnedDeployment.PackageType);
                }
                else
                {
                    Assert.Equal(PackageType.DeviceConfiguration, returnedDeployment.PackageType);
                }
            }
        }

        [Theory]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetDeploymentMetricsTest(bool isEdgeDeployment)
        {
            // Arrange
            var content = new ConfigurationContent()
            {
                ModulesContent = isEdgeDeployment ? new Dictionary<string, IDictionary<string, object>>() : null,
                DeviceContent = !isEdgeDeployment ? new Dictionary<string, object>() : null
            };

            var label = isEdgeDeployment ? PackageType.EdgeManifest.ToString() : PackageType.DeviceConfiguration.ToString();

            var Firmware = "Firmware";

            var configuration = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, string.Empty },
                    { DEPLOYMENT_GROUP_ID_LABEL, string.Empty },
                    { packageTypeLabel, label },
                    { CONFIG_TYPE_LABEL, Firmware },
                    { RM_CREATED_LABEL, bool.TrueString },
                },
                Content = content
            };

            var deploymentId = configuration.Id;
            this.registry.Setup(r => r.GetConfigurationAsync(deploymentId)).ReturnsAsync(configuration);
            this.registry.Setup(r => r.CreateQuery(It.IsAny<string>())).Returns(new ResultQuery(0));
            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            // Act
            var returnedDeployment = await this.deployments.GetAsync(deploymentId);

            // Assert Should return Deplyment metrics according to label
            Assert.NotNull(returnedDeployment.DeploymentMetrics.DeviceMetrics);
            Assert.Equal(3, returnedDeployment.DeploymentMetrics.DeviceMetrics.Count());
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyGroupAndPackageNameLabelsTest()
        {
            // Arrange
            var deviceGroupId = "dvcGroupId";
            var deviceGroupName = "dvcGroupName";
            var deviceGroupQuery = "dvcGroupQuery";
            var packageName = "packageName";
            var deploymentName = "depName";
            var priority = 10;
            var depModel = new DeploymentServiceModel()
            {
                Name = deploymentName,
                DeviceGroupId = deviceGroupId,
                DeviceGroupName = deviceGroupName,
                DeviceGroupQuery = deviceGroupQuery,
                PackageContent = TEST_EDGE_PACKAGE_JSON,
                PackageName = packageName,
                Priority = priority
            };

            var newConfig = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { packageTypeLabel, PackageType.EdgeManifest.ToString() },
                    { DEPLOYMENT_NAME_LABEL, deploymentName },
                    { DEPLOYMENT_GROUP_ID_LABEL, deviceGroupId },
                    { RM_CREATED_LABEL, bool.TrueString },
                    { DEPLOYMENT_GROUP_NAME_LABEL, deviceGroupName },
                    { DEPLOYMENT_PACKAGE_NAME_LABEL, packageName }
                },
                Priority = priority
            };

            this.registry.Setup(r => r.AddConfigurationAsync(It.Is<Configuration>(c =>
                    c.Labels.ContainsKey(DEPLOYMENT_NAME_LABEL) &&
                    c.Labels.ContainsKey(DEPLOYMENT_GROUP_ID_LABEL) &&
                    c.Labels.ContainsKey(RM_CREATED_LABEL) &&
                    c.Labels[DEPLOYMENT_NAME_LABEL] == deploymentName &&
                    c.Labels[DEPLOYMENT_GROUP_ID_LABEL] == deviceGroupId &&
                    c.Labels[RM_CREATED_LABEL] == bool.TrueString)))
                .ReturnsAsync(newConfig);

            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            // Act
            var createdDeployment = await this.deployments.CreateAsync(depModel);

            // Assert
            Assert.False(string.IsNullOrEmpty(createdDeployment.Id));
            Assert.Equal(deploymentName, createdDeployment.Name);
            Assert.Equal(deviceGroupId, createdDeployment.DeviceGroupId);
            Assert.Equal(priority, createdDeployment.Priority);
            Assert.Equal(deviceGroupName, createdDeployment.DeviceGroupName);
            Assert.Equal(packageName, createdDeployment.PackageName);
        }

        [Fact]
        [Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task FilterOutNonRmDeploymentsTest()
        {
            // Arrange
            var configurations = new List<Configuration>
            {
                this.CreateConfiguration(0, true),
                this.CreateConfiguration(1, false)
            };

            this.registry.Setup(r => r.GetConfigurationsAsync(20))
                .ReturnsAsync(configurations);
            tenantHelper.Setup(e => e.GetRegistry()).Returns(this.registry.Object);

            // Act
            var returnedDeployments = await this.deployments.ListAsync();

            // Assert
            Assert.Single(returnedDeployments.Items);
            Assert.Equal("deployment0", returnedDeployments.Items[0].Name);
        }

        private Configuration CreateConfiguration(int idx, bool addCreatedByRmLabel)
        {
            var conf = new Configuration("test-config" + idx)
            {
                Labels = new Dictionary<string, string>()
                {
                    { packageTypeLabel, PackageType.EdgeManifest.ToString() },
                    { DEPLOYMENT_NAME_LABEL, "deployment" + idx },
                    { DEPLOYMENT_GROUP_ID_LABEL, "dvcGroupId" + idx }
                },
                Priority = 10
            };

            if (addCreatedByRmLabel)
            {
                conf.Labels.Add(RM_CREATED_LABEL, "true");
            }

            return conf;
        }
    }
}
