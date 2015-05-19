using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils.Guids;
using Journey.Utils.SystemTime;
using SimpleInventario.Commands;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using Xunit;

namespace SimpleInventario.Tests.Performance.InventarioPerformanceFixture
{
    public class DADO_bus
    {
        protected CommandBus bus;

        public DADO_bus()
        {
            var messageSender = new MessageSender(Database.DefaultConnectionFactory, "server=(local);Database=Journey;User Id=sa;pwd =123456", "Bus.Commands");
            //var messageSender = new MessageSender(Database.DefaultConnectionFactory, "Data Source=.\\sqlexpress;Initial Catalog=journey;Integrated Security=True", "Bus.Commands");
            this.bus = new CommandBus(messageSender, new JsonTextSerializer(), new LocalDateTime());
        }

        [Fact]
        public void CUANDO_se_envian_comandos_masivamente_ENTONCES_se_procesan_rapidamente()
        {
            var guid = new SequentialGuid();
            var commandsToSendCount = 3;

            var commands = new List<ICommand>();
            for (int i = 0; i < commandsToSendCount; i++)
            {
                commands.Add(new AgregarAnimales(
                        guid.NewGuid(),
                        Guid.Empty,
                        Guid.Parse("00000000-0000-0000-0000-000000000000"),
                        Guid.Parse("00000000-0000-0000-0000-000000000005"),
                        1,
                        2015
                    ));
                commands.Add(new AgregarAnimales(
                        guid.NewGuid(),
                        Guid.Empty,
                        Guid.Parse("00000000-0000-0000-0000-000000000000"),
                        Guid.Parse("00000000-0000-0000-0000-000000000005"),
                        -1,
                        2015
                    ));
                commands.Add(new AgregarAnimales(
                        guid.NewGuid(),
                        Guid.Empty,
                        Guid.Parse("00000000-0000-0000-0000-000000000000"),
                        Guid.Parse("00000000-0000-0000-0000-000000000005"),
                        1,
                        2014
                ));
            }

            this.bus.Send(commands);
        }
    }
}
