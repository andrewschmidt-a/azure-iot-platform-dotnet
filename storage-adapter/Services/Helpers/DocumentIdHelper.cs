// <copyright file="DocumentIdHelper.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Iot.StorageAdapter.Services.Helpers
{
    public static class DocumentIdHelper
    {
        /// <summary>
        /// To reduce cost, we will use single document collection. So the actual document
        /// ID will be composed by the "logical" collectionId and key
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="key"></param>
        /// <returns>Generated document ID</returns>
        public static string GenerateId(string collectionId, string key)
        {
            return $"{collectionId.ToLowerInvariant()}.{key.ToLowerInvariant()}";
        }
    }
}