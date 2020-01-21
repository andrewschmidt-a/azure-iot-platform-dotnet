// <copyright file="ValueModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Platform.IoT.IoTHubManager.Services.Test
{
    public partial class StorageWriteLockTest
    {
        private class ValueModel
        {
            public string Value { get; set; }

            public bool Locked { get; set; }
        }
    }
}