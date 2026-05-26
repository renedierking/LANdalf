using Serilog;

namespace API.Services {
    public static class SerilogFileLoggingConfigurator {
        private const string FileSectionPath = "Serilog:File";
        private const string DefaultPath = "logs/landalf-.log";
        private const string DefaultRollingInterval = "Day";
        private const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

        public static bool TryConfigureFileSink(LoggerConfiguration loggerConfiguration, IConfiguration configuration) {
            ArgumentNullException.ThrowIfNull(loggerConfiguration);
            ArgumentNullException.ThrowIfNull(configuration);

            IConfigurationSection fileSection = configuration.GetSection(FileSectionPath);
            if (!fileSection.GetValue("Enabled", false)) {
                return false;
            }

            string path = fileSection["Path"] ?? DefaultPath;
            string rollingIntervalValue = fileSection["RollingInterval"] ?? DefaultRollingInterval;
            if (!Enum.TryParse(rollingIntervalValue, ignoreCase: true, out RollingInterval rollingInterval)) {
                rollingInterval = RollingInterval.Day;
            }

            int? retainedFileCountLimit = fileSection.GetValue<int?>("RetainedFileCountLimit");
            string outputTemplate = fileSection["OutputTemplate"] ?? DefaultOutputTemplate;

            loggerConfiguration.WriteTo.File(
                path: path,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: retainedFileCountLimit,
                outputTemplate: outputTemplate);

            return true;
        }
    }
}
