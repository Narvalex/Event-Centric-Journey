using System;
using System.Configuration;

namespace Journey.Client
{
    /// <summary>
    /// El Default Config entiende que sólo se utiliza una única base de datos.
    /// </summary>
    public class DefaultClientApplicationConfigProvider : ConfigurationSection, IClientApplicationConfig
    {
        private const string sectionName = "clientConfig";
        private const string connectionString = "connectionString";
        private const string workerRoleStatusUrl = "workerRoleStatusUrl";
        private const string eventualConsistencyCheckRetryPolicy = "eventualConsistencyCheckRetryPolicy";


        private DefaultClientApplicationConfigProvider()
        { }

        public static IClientApplicationConfig Configuration
        {
            get { return ConfigurationManager.GetSection(sectionName) as DefaultClientApplicationConfigProvider; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string BusConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string ReadModelConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(workerRoleStatusUrl, IsRequired = true)]
        public string WorkerRoleStatusUrl
        {
            get { return this[workerRoleStatusUrl] as string; }
        }

        [ConfigurationProperty(eventualConsistencyCheckRetryPolicy, IsRequired = true)]
        public int EventualConsistencyCheckRetryPolicy
        {
            get { return Convert.ToInt32(this[eventualConsistencyCheckRetryPolicy]); }
        }


        public string CommandBusTableName
        {
            get { return "Bus.Commands"; }
        }

        public string EventBusTableName
        {
            get { return "Bus.Events"; }
        }
    }
}
