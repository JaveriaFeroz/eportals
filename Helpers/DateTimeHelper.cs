using System;

namespace ProcureToPay.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetPakistanStandardTime()
        {
            try
            {
                // Find the Pakistan Standard Time zone
                // Note: The ID might be "Asia/Karachi" on non-Windows systems
                var pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

                // Convert UTC time to PST
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Handle cases where the time zone is not found on the system
                // Fallback to UTC or a default time
                return DateTimeHelper.GetPakistanStandardTime();
            }
        }
    }
}