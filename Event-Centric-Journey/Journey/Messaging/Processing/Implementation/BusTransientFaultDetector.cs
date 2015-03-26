using Journey.Database;
using System.Data.SqlClient;
using System.Linq;

namespace Journey.Messaging.Processing
{
    public class BusTransientFaultDetector : IBusTransientFaultDetector
    {
        private readonly SqlCommandWrapper sql;

        public BusTransientFaultDetector(string connectionString)
        {
            this.sql = new SqlCommandWrapper(connectionString);
        }

        public bool CommandWasAlreadyProcessed(object payload)
        {
            return this.sql.ExecuteReader(@"
            select count(*) from  EventStore.Events where TaskCommandId = @CommandId
            ", r => r.SafeGetInt32(0) > 0 ? true : false,
                new SqlParameter("@CommandId", ((dynamic)payload).Id)).FirstOrDefault();
        }
    }
}
