// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Config.Services.External;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Config;

namespace Mmm.Platform.IoT.Config.Services.Models.Actions
{
    public class EmailActionSettings : IActionSettings
    {
        private const string IS_ENABLED_KEY = "IsEnabled";
        private const string OFFICE365_CONNECTOR_URL_KEY = "Office365ConnectorUrl";
        private const string APP_PERMISSIONS_KEY = "ApplicationPermissionsAssigned";

        private readonly IAzureResourceManagerClient resourceManagerClient;
        private readonly AppConfig config;
        private readonly ILogger _logger;

        // In order to initialize all settings, call InitializeAsync
        // to retrieve all settings due to async call to logic app
        public EmailActionSettings(
            IAzureResourceManagerClient resourceManagerClient,
            AppConfig config,
            ILogger<EmailActionSettings> logger)
        {
            this.resourceManagerClient = resourceManagerClient;
            this.config = config;
            _logger = logger;

            this.Type = ActionType.Email;
            this.Settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ActionType Type { get; }

        public IDictionary<string, object> Settings { get; set; }

        public async Task InitializeAsync()
        {
            // Check signin status of Office 365 Logic App Connector
            var office365IsEnabled = false;
            var applicationPermissionsAssigned = true;
            try
            {
                office365IsEnabled = await this.resourceManagerClient.IsOffice365EnabledAsync();
            }
            catch (NotAuthorizedException notAuthorizedException)
            {
                // If there is a 403 Not Authorized exception, it means the application has not
                // been given owner permissions to make the isEnabled check. This can be configured
                // by an owner in the Azure Portal.
                applicationPermissionsAssigned = false;
                _logger.LogError(notAuthorizedException, "The application is not authorized and has not been assigned owner permissions for the subscription. Go to the Azure portal and assign the application as an owner in order to retrieve the token.");
            }
            this.Settings.Add(IS_ENABLED_KEY, office365IsEnabled);
            this.Settings.Add(APP_PERMISSIONS_KEY, applicationPermissionsAssigned);

            // Get Url for Office 365 Logic App Connector setup in portal
            // for display on the webui for one-time setup.
            this.Settings.Add(OFFICE365_CONNECTOR_URL_KEY, config.ConfigService.ConfigServiceActions.Office365ConnectionUrl);

            _logger.LogDebug("Email action settings retrieved: {settings}. Email setup status: {status}", office365IsEnabled, Settings);
        }
    }
}
