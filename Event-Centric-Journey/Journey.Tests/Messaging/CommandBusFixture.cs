using Journey.Messaging;
using Journey.Serialization;
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
            this.sut = new CommandBus(fakeSender, this.serializerMock.Object);
        }

        [Fact]
        public void THEN_is_not_null()
        {
            Assert.NotNull(this.sut);
        }

    }

    public class FakeSender : IMessageSender, ISqlBus
    {
        public void Send(Message message)
        {
            throw new NotImplementedException();
        }

        public void Send(IEnumerable<Message> messages)
        {
            throw new NotImplementedException();
        }

        public string TableName
        {
            get { throw new NotImplementedException(); }
        }


        public void Send(IEnumerable<Message> messages, DbContext connection)
        {
            throw new NotImplementedException();
        }
    }
}
