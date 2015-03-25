using Infrastructure.CQRS.Serialization;
using System;

namespace Infrastructure.CQRS.Messaging
{
    public abstract class SqlBus : ISqlBus
    {
        protected IMessageSender sender;
        protected ITextSerializer serializer;

        protected SqlBus(IMessageSender sender, ITextSerializer serializer)
        {
            if (!typeof(ISqlBus).IsAssignableFrom((sender.GetType())))
                throw new InvalidCastException(
                    "El sender debe implementar ISqlBus para poder incluirlo en transacciones distribuidas al bus");

            this.sender = sender;
            this.serializer = serializer;
        }

        /// <summary>
        /// The Table Name implemented for messaging capabilities
        /// </summary>
        public string TableName { get { return (this.sender as ISqlBus).TableName; } }
    }
}
