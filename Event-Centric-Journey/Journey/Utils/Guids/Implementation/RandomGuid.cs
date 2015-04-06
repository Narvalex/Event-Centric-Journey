using System;

namespace Journey.Utils.Guids
{
    public class RandomGuid : IGuidGenerator
    {
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
