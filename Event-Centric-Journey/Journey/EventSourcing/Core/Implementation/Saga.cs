using Journey.Messaging;
using System;
using System.Collections.Generic;

namespace Journey.EventSourcing
{
    /// <summary>
    /// A Saga aggregate that publishes commands to the bus.
    /// </summary>
    /// <remarks>
    /// Feels ankward and possibly disrupting to store POCOs (Plane Old CLR Objects) in the <see cref="ISaga"/> aggregate 
    /// implementor. Maybe it would be better if instead of using current sate values (properties in C# and columns in the SQL Database),
    /// we use event sourcing.
    /// </remarks>
    public abstract class Saga : ComplexEventSourced, ISaga
    {
        private readonly List<ICommand> commands = new List<ICommand>();

        protected Saga(Guid id)
            : base(id)
        { }

        public IEnumerable<ICommand> Commands { get { return this.commands; } }

        protected void AddCommand<T>(T command) where T : ICommand
        {
            this.commands.Add(command);
        }
    }
}
