using System;
using Newtonsoft.Json.Linq;

namespace Mmm.Platform.IoT.Config.Services.Helpers.PackageValidation
{
    public interface IPackageValidator
    {
        JObject ParsePackageContent(string package);
        bool Validate();
    }
}
