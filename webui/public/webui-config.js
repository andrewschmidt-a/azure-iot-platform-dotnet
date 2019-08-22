// Copyright (c) Microsoft. All rights reserved.

// This file is rewritten during the deployment.
// Values can be changed for development purpose.
// The file is public, so don't expect secrets here.

var DeploymentConfig = {
  authEnabled: true,
  authType: 'aad',
  aad : {
    // tenant: 'facac3c4-e2a5-4257-af76-205c8a821ddb',
    tenant: 'crsltestab2c.onmicrosoft.com',
    // appId: '95d3c662-23ea-4e2d-8d3d-ea2448706934',
    appId: '6e2a6569-f0b9-4a2b-a75c-5ec6c80869eb',
    instance: 'https://login.microsoftonline.com'
  },
  issuer: "https://crsliotkubedev.centralus.cloudapp.azure.com/auth/"
}
