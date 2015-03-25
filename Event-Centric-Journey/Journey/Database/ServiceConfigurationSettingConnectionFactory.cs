using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Infrastructure.CQRS.Database
{
    public class ServiceConfigurationSettingConnectionFactory : IDbConnectionFactory
    {
        private readonly object lockObject = new object();
        private readonly IDbConnectionFactory parent;
        private Dictionary<string, string> cachedConnectionStringsMap = new Dictionary<string, string>();

        public ServiceConfigurationSettingConnectionFactory(IDbConnectionFactory parent)
        {
            this.parent = parent;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            string connectionString = null;
            if (!IsConnectionString(nameOrConnectionString))
            {
                if (!this.cachedConnectionStringsMap.TryGetValue(nameOrConnectionString, out connectionString))
                {
                    lock (this.lockObject)
                    {
                        if (!this.cachedConnectionStringsMap.TryGetValue(nameOrConnectionString, out connectionString))
                        {
                            var connectionStringName = "DbContext." + nameOrConnectionString;

                            try
                            {
                                var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
                                if (connectionStringSettings != null)
                                {
                                    connectionString = connectionStringSettings.ConnectionString;
                                }
                            }
                            catch (ConfigurationErrorsException)
                            {
                            }

                            var immutableDictionary = this.cachedConnectionStringsMap
                                .Concat(new[] { new KeyValuePair<string, string>(nameOrConnectionString, connectionString) })
                                .ToDictionary(x => x.Key, x => x.Value);

                            this.cachedConnectionStringsMap = immutableDictionary;
                        }
                    }
                }
            }

            if (connectionString == null)
            {
                connectionString = nameOrConnectionString;
            }

            return this.parent.CreateConnection(connectionString);
        }

        private static bool IsConnectionString(string connectionStringCandidate)
        {
            return (connectionStringCandidate.IndexOf('=') >= 0);
        }
    }
}
