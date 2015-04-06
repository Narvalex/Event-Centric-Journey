using System;

namespace Journey.Utils.SystemDateTime
{
    public class LocalDateTime : ISystemDateTime
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
