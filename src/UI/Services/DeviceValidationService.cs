using System.Net;
using System.Text.RegularExpressions;

namespace LANdalf.UI.Services {
    /// <summary>
    /// Implementation of device validation service.
    /// </summary>
    public partial class DeviceValidationService : IDeviceValidationService {
        private const int MaxNameLength = 64;

        /// <inheritdoc />
        public string? ValidateName(string? name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return "Name is required";
            }

            if (name.Length > MaxNameLength) {
                return $"Name must be {MaxNameLength} characters or less";
            }

            return null;
        }

        /// <inheritdoc />
        public string? ValidateMacAddress(string? macAddress) {
            if (string.IsNullOrWhiteSpace(macAddress)) {
                return "MAC Address is required";
            }

            // Accept three formats:
            // 1. XX:XX:XX:XX:XX:XX (colon-separated)
            // 2. XX-XX-XX-XX-XX-XX (hyphen-separated)
            // 3. XXXXXXXXXXXX (12 hex characters, no separator)
            if (!MacAddressRegex().IsMatch(macAddress)) {
                return "Invalid MAC Address format. Use XX:XX:XX:XX:XX:XX, XX-XX-XX-XX-XX-XX, or XXXXXXXXXXXX";
            }

            return null;
        }

        /// <inheritdoc />
        public string? ValidateIpAddress(string? ipAddress) {
            // Optional field - empty is valid
            if (string.IsNullOrWhiteSpace(ipAddress)) {
                return null;
            }

            if (!IPAddress.TryParse(ipAddress, out _)) {
                return "Invalid IP Address format";
            }

            return null;
        }

        [GeneratedRegex(@"^[0-9A-Fa-f]{2}(?<sep>[:\-])([0-9A-Fa-f]{2}\k<sep>){4}[0-9A-Fa-f]{2}$|^[0-9A-Fa-f]{12}$", RegexOptions.Compiled)]
        private static partial Regex MacAddressRegex();
    }
}
