using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils.SystemTime;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using Xunit;

namespace Journey.Tests.Messaging.CommandBusFixture
{
    public class GIVEN_a_command_bus
    {
        IMessageSender fakeSender = new FakeSender();
        Mock<ITextSerializer> serializerMock = new Mock<ITextSerializer>();
        private readonly ICommandBus sut;

        public GIVEN_a_command_bus()
        {
            this.sut = new CommandBus(fakeSender, this.serializerMock.Object, new LocalDateTime());
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


        public void Send(IEnumerable<MessageForDelivery> messages, DbContext connection)
        {
            throw new NotImplementedException();
        }
    }
}
