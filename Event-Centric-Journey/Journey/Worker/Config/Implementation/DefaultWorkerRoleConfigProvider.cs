using System;
using System.Configuration;

namespace Journey.Worker.Config
{
    /// <summary>
    /// El Default Config entiende que sólo se utiliza una única base de datos.
    /// </summary>
    public class DefaultWorkerRoleConfigProvider : ConfigurationSection, IWorkerRoleConfig, IReadModelRebuilderConfig, IEventStoreRebuilderConfig
    {
        private const string sectionName = "workerConfig";
        private const string connectionString = "connectionString";
        private const string busPollDelay = "busPollDelay";
        private const string numberOfProcessorThreads = "numberOfProcessorThreads";

        private DefaultWorkerRoleConfigProvider()
        { }

        public static DefaultWorkerRoleConfigProvider Configuration
        {
            get { return ConfigurationManager.GetSection(sectionName) as DefaultWorkerRoleConfigProvider; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string EventStoreConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string MessageLogConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string SourceMessageLogConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string NewMessageLogConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(connectionString, IsRequired = true)]
        public string ReadModelConnectionString
        {
            get { return this[connectionString] as string; }
        }

        [ConfigurationProperty(numberOfProcessorThreads, IsRequired = true)]
        public int NumberOfProcessorsThreads
        {
            get { return Convert.ToInt32(this[numberOfProcessorThreads]); }
        }

        [ConfigurationProperty(busPollDelay, IsRequired = true)]
        public int BusPollDelaySetting
        {
            get { return Convert.ToInt32(this[busPollDelay]); }
        }

        public TimeSpan BusPollDelay
        {
            get { return new TimeSpan(0, 0, 0, 0, this.BusPollDelaySetting); }
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
