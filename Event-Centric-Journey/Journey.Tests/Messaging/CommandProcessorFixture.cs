using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Worker;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Journey.Tests.Messaging.CommandProcessorFixture
{
    public class GIVEN_command_processor
    {
        private Mock<IMessageReceiver> receiverMock = new Mock<IMessageReceiver>();
        private CommandProcessor processor;

        public GIVEN_command_processor()
        {
            this.processor = new CommandProcessor(this.receiverMock.Object, CreateSerializer(), new ConsoleTracer(), new FakeFaultDetector());
        }

        [Fact]
        public void WHEN_starting_THEN_starts_receiver()
        {
            this.processor.Start();

            this.receiverMock.Verify(r => r.Start());
        }

        [Fact]
        public void WHEN_stopping_after_starting_THEN_stops_receiver()
        {
            this.processor.Start();
            this.processor.Stop();

            this.receiverMock.Verify(r => r.Stop());
        }

        /// <summary>
        /// Todo: Hacer el comprobador un componente inyectable.
        /// </summary>
        [Fact]
        public void WHEN_receives_message_THEN_notifies_registered_handler()
        {
            var handlerAMock = new Mock<ICommandHandler>();
            handlerAMock.As<ICommandHandler<Command1>>();

            var handlerBMock = new Mock<ICommandHandler>();
            handlerBMock.As<ICommandHandler<Command2>>();

            this.processor.Register(handlerAMock.Object);
            this.processor.Register(handlerBMock.Object);

            this.processor.Start();

            var command1 = new Command1 { Id = Guid.NewGuid() };
            var command2 = new Command2 { Id = Guid.NewGuid() };

            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new MessageForDelivery(Serialize(command1))));
            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new MessageForDelivery(Serialize(command2))));

            handlerAMock.As<ICommandHandler<Command1>>().Verify(h => h.Handle(It.Is<Command1>(e => e.Id == command1.Id)));
            handlerBMock.As<ICommandHandler<Command2>>().Verify(h => h.Handle(It.Is<Command2>(e => e.Id == command2.Id)));
        }

        [Fact]
        public void WHEN_throws_THEN_retries()
        {
            this.processor.Register(new BuggyCommandHandler());

            this.processor.Start();

            var command1 = new Command1 { Id = Guid.NewGuid() };

            Assert.Throws(new Exception().GetType(), 
                () => this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new MessageForDelivery(Serialize(command1)))));
        }

        private string Serialize(object payload)
        {
            var serializer = CreateSerializer();

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, payload);
                return writer.ToString();
            }
        }

        private static ITextSerializer CreateSerializer()
        {
            return new JsonTextSerializer();
        }

        public class Command1 : ICommand
        {
            public Guid Id { get; set; }


            public DateTime TimeStamp { get; set; }

            public DateTime CreationDate
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class Command2 : ICommand
        {
            public Guid Id { get; set; }


            public DateTime TimeStamp { get; set; }

            public DateTime CreationDate
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class BuggyCommandHandler : 
            ICommandHandler<Command1>
        {

            public void Handle(Command1 command)
            {
                Console.WriteLine("Throwing error while handling a message");
                throw new Exception();
            }
        }

    }

    public class FakeFaultDetector : ICommandBusTransientFaultDetector
    {
        public bool CommandWasAlreadyProcessed(object payload)
        {
            return false;
        }
    }
}
