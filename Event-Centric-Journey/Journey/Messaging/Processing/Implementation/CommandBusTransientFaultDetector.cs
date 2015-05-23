using Journey.Database;
using System.Data.SqlClient;
using System.Linq;

namespace Journey.Messaging.Processing
{
    public class CommandBusTransientFaultDetector : IBusTransientFaultDetector
    {
        private readonly SqlCommandWrapper sql;

        public CommandBusTransientFaultDetector(string connectionString)
        {
            this.sql = new SqlCommandWrapper(connectionString);
        }

        public bool MessageWasAlreadyProcessed(object payload)
        {
            return this.sql.ExecuteReader(@"
            select count(*) from  
            MessageLog.Messages where 
            SourceId = @CommandId
            and FullName = @FullName
            ", r => r.SafeGetInt32(0) > 0 ? true : false,
                new SqlParameter("@CommandId", ((dynamic)payload).Id),
                new SqlParameter("@FullName", payload.GetType().FullName))
            .FirstOrDefault();
        }
    }
}
