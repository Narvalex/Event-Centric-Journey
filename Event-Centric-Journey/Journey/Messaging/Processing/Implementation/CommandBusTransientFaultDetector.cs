using Journey.Database;
using System.Data.SqlClient;
using System.Linq;

namespace Journey.Messaging.Processing
{
    public class CommandBusTransientFaultDetector : ICommandBusTransientFaultDetector
    {
        private readonly SqlCommandWrapper sql;

        public CommandBusTransientFaultDetector(string connectionString)
        {
            this.sql = new SqlCommandWrapper(connectionString);
        }

        public bool CommandWasAlreadyProcessed(object payload)
        {
            return this.sql.ExecuteReader(@"
            select count(*) from  EventStore.Events where CorrelationId = @CommandId
            ", r => r.SafeGetInt32(0) > 0 ? true : false,
                new SqlParameter("@CommandId", ((dynamic)payload).Id)).FirstOrDefault();
        }
    }
}
