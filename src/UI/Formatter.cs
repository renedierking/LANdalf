namespace LANdalf.UI {
    public static class Formatter {
        public static string FormatOnlineSince(DateTimeOffset onlineSince) {
            var duration = DateTimeOffset.UtcNow - onlineSince;

            if (duration.TotalMinutes < 1) {
                return "just now";
            } else if (duration.TotalMinutes < 60) {
                var minutes = (int)duration.TotalMinutes;
                return $"{minutes} minute{(minutes > 1 ? "s" : "")} ago";
            } else if (duration.TotalHours < 24) {
                var hours = (int)duration.TotalHours;
                return $"{hours} hour{(hours > 1 ? "s" : "")} ago";
            } else {
                var days = (int)duration.TotalDays;
                return $"{days} day{(days > 1 ? "s" : "")} ago";
            }
        }
    }
}
