using Journey.Messaging;
using Journey.Serialization;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using Xunit;

namespace Journey.Tests.Messaging.EventBusFixture
{
    public class GIVEN_an_event_bus
    {
        IMessageSender fakeSender = new FakeSender();
        Mock<ITextSerializer> serializerMock = new Mock<ITextSerializer>();
        private readonly IEventBus sut;

        public GIVEN_an_event_bus()
        {
            this.sut = new EventBus(fakeSender, this.serializerMock.Object);
        }

        [Fact]
        public void THEN_is_not_null()
        {
            Assert.NotNull(this.sut);
        }
    }

    public class FakeSender : IMessageSender, ISqlBus
    {
        public void Send(MessageForDelivery message)
        {
            throw new NotImplementedException();
        }

        public void Send(IEnumerable<MessageForDelivery> messages)
        {
            throw new NotImplementedException();
        }

        public string TableName
        {
            get { throw new NotImplementedException(); }
        }


        public void Send(IEnumerable<MessageForDelivery> messages, DbContext context)
        {
            throw new NotImplementedException();
        }
    }
}
