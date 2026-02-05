namespace LANdalf.UI.Services {
    /// <summary>
    /// Service for validating PC device fields.
    /// </summary>
    public interface IDeviceValidationService {
        /// <summary>
        /// Validates the device name.
        /// </summary>
        /// <param name="name">The device name to validate.</param>
        /// <returns>Null if valid, otherwise an error message.</returns>
        string? ValidateName(string? name);

        /// <summary>
        /// Validates the MAC address.
        /// Accepts formats: XX:XX:XX:XX:XX:XX, XX-XX-XX-XX-XX-XX, or XXXXXXXXXXXX
        /// </summary>
        /// <param name="macAddress">The MAC address to validate.</param>
        /// <returns>Null if valid, otherwise an error message.</returns>
        string? ValidateMacAddress(string? macAddress);

        /// <summary>
        /// Validates an optional IP address field.
        /// </summary>
        /// <param name="ipAddress">The IP address to validate (can be null or empty).</param>
        /// <returns>Null if valid or empty, otherwise an error message.</returns>
        string? ValidateIpAddress(string? ipAddress);
    }
}
