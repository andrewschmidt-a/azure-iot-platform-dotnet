using System.Text.RegularExpressions;
using Mmm.Platform.IoT.Common.Services.Exceptions;

namespace Mmm.Platform.IoT.Common.Services.Helpers
{
    public class InputValidator
    {
        private const string InvalidCharacterRegex = @"[^A-Za-z0-9:;.!,_\-*@ ]";

        // Check illegal characters in input
        public static void Validate(string input)
        {
            if (Regex.IsMatch(input.Trim(), InvalidCharacterRegex))
            {
                throw new InvalidInputException($"Input '{input}' contains invalid characters.");
            }
        }
    }
}