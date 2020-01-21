namespace Mmm.Platform.IoT.Config.Services.Helpers.PackageValidation
{
    internal class FirmwareValidator : PackageValidator, IPackageValidator
    {
        // TODO: Implement validation for Firmware Update for MxChip packages
        public override bool Validate()
        {
            return true;
        }
    }
}