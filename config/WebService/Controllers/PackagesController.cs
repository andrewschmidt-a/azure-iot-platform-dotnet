// <copyright file="PackagesController.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mmm.Iot.Common.Services;
using Mmm.Iot.Common.Services.Exceptions;
using Mmm.Iot.Common.Services.Filters;
using Mmm.Iot.Config.Services;
using Mmm.Iot.Config.Services.Models;
using Mmm.Iot.Config.WebService.Helpers;
using Mmm.Iot.Config.WebService.Models;

namespace Mmm.Iot.Config.WebService.Controllers
{
    [Route("v1/packages")]
    [TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class PackagesController : Controller
    {
        private readonly IStorage storage;

        public PackagesController(IStorage storage)
        {
            this.storage = storage;
        }

        /**
         * This function can be used to get packages with or without parameters
         * PackageType, ConfigType. Without both the query params this will return all
         * the packages. With only packageType the method will return packages of that
         * packageType. If only configType is provided (w/o package type) the method will
         * throw an Exception.
         */
        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<PackageListApiModel> GetFilteredAsync(
            [FromQuery]string packageType,
            [FromQuery]string configType)
        {
            if (string.IsNullOrEmpty(packageType) && string.IsNullOrEmpty(configType))
            {
                return new PackageListApiModel(await this.storage.GetAllPackagesAsync());
            }

            if (string.IsNullOrEmpty(packageType))
            {
                throw new InvalidInputException("Valid packageType must be provided");
            }

            return new PackageListApiModel(await this.storage.GetFilteredPackagesAsync(
                packageType,
                configType));
        }

        [HttpGet("{id}")]
        [Authorize("ReadAll")]
        public async Task<PackageApiModel> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidInputException("Valid id must be provided");
            }

            return new PackageApiModel(await this.storage.GetPackageAsync(id));
        }

        [HttpPost]
        [Authorize("CreatePackages")]
        public async Task<PackageApiModel> PostAsync(string packageType, string configType, IFormFile package)
        {
            if (string.IsNullOrEmpty(packageType))
            {
                throw new InvalidInputException("Package Type must be provided");
            }

            bool isValidPackageType = Enum.TryParse(packageType, true, out PackageType uploadedPackageType);
            if (!isValidPackageType)
            {
                throw new InvalidInputException($"Provided packageType {packageType} is not valid.");
            }

            if (uploadedPackageType.Equals(PackageType.EdgeManifest) && !string.IsNullOrEmpty(configType))
            {
                throw new InvalidInputException($"Package of type EdgeManifest cannot have parameter " +
                    $"configType.");
            }

            if (configType == null)
            {
                configType = string.Empty;
            }

            if (package == null || package.Length == 0 || string.IsNullOrEmpty(package.FileName))
            {
                throw new InvalidInputException("Package uploaded is missing or invalid.");
            }

            string packageContent;
            using (var streamReader = new StreamReader(package.OpenReadStream()))
            {
                packageContent = await streamReader.ReadToEndAsync();
            }

            if (!PackagesHelper.VerifyPackageType(packageContent, uploadedPackageType))
            {
                throw new InvalidInputException($@"Package uploaded is invalid. Package contents
                            do not match with the given package type {packageType}.");
            }

            var packageToAdd = new PackageApiModel(
                packageContent,
                package.FileName,
                uploadedPackageType,
                configType);

            return new PackageApiModel(await this.storage.AddPackageAsync(packageToAdd.ToServiceModel()));
        }

        [HttpDelete("{id}")]
        [Authorize("DeletePackages")]
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidInputException("Valid id must be provided");
            }

            await this.storage.DeletePackageAsync(id);
        }

        [HttpPost("UploadFile")]
        [Authorize("CreatePackages")]
        public async Task<string> UploadFileAsync(IFormFile uploadedFile)
        {
            string uploadedUri = null;
            if (uploadedFile == null || uploadedFile.Length == 0 || string.IsNullOrEmpty(uploadedFile.FileName))
            {
                throw new InvalidInputException("Uploaded file is missing or invalid.");
            }

            using (var stream = uploadedFile.OpenReadStream())
            {
                var tenantId = this.GetTenantId();
                uploadedUri = await this.storage.UploadToBlobAsync(tenantId, uploadedFile.FileName, stream);
            }

            return uploadedUri;
        }
    }
}