using System;
using System.Configuration;

namespace Journey.Worker.Config
{
    public class DefaultConfigProvider : ConfigurationSection, IWorkerRoleConfig
    {
        private const string sectionName = "workerConfig";
        private const string connectionString = "connectionString";
        private const string busPollDelay = "busPollDelay";
        private const string numberOfProcessorThreads = "numberOfProcessorThreads";

        private DefaultConfigProvider()
        { }

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

        public static IWorkerRoleConfig Configuration 
        {
            get { return ConfigurationManager.GetSection(sectionName) as DefaultConfigProvider; }
        }


        public TimeSpan BusPollDelay
        {
            get { return new TimeSpan(0, 0, 0, 0, this.BusPollDelaySetting); }
        }
    }
}
