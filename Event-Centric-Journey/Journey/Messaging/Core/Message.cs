using System;

namespace Infrastructure.CQRS.Messaging
{
    public class Message
    {
        public Message(string body, string correlationId = null, DateTime? deliveryDate = null)
        {
            this.Body = body;
            this.CorrelationId = correlationId;
            this.DeliveryDate = deliveryDate;
        }

        public string Body { get; private set; }

        public string CorrelationId { get; private set; }

        public DateTime? DeliveryDate { get; private set; }
    }
}
