using System;

namespace Journey.Utils.SystemTime
{
    public class LocalDateTime : ISystemTime
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        public DateTimeOffset NowOffset
        {
            get
            {
                return DateTimeOffset.Now;
            }
        }
    }
}
