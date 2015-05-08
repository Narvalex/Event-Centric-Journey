using System;

namespace Journey.Utils.SystemTime
{
    public class UtcDateTime : ISystemTime
    {
        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public DateTimeOffset NowOffset
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }
}
