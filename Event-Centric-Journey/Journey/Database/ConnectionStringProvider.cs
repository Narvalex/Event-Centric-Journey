using System.Web.Configuration;

namespace Journey.Database
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly string connectionString;

        /// <summary>
        /// Creates an instance of <see cref="ConnectionStringProvider"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to expose.</param>
        public ConnectionStringProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Creates an instance of <see cref="ConnectionStringProvider"/>
        /// </summary>
        /// <param name="configPath">The virtual path to the configuration file. If null, the root Web.config file is opened. 
        /// For example: "/PedidosOnlineAdmin.Web"</param>
        /// <param name="configName">The name of the configuation object of the connection string collection. 
        /// For example: "PedidosOnlineAdminConnectionString"</param>
        /// <remarks>This constructor is based on: http://msdn.microsoft.com/en-us/library/ms178411(v=vs.100).aspx </remarks>
        public ConnectionStringProvider(string configPath, string configName)
        {
            var rootWebConfig = WebConfigurationManager.OpenWebConfiguration(configPath);

            System.Configuration.ConnectionStringSettings connectionString;

            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                connectionString = rootWebConfig.ConnectionStrings.ConnectionStrings[configName];
                if (connectionString == null)
                    throw new System.ArgumentNullException(connectionString.ToString());

                this.connectionString = connectionString.ConnectionString;
            }
            else
            {
                throw new System.ArgumentNullException();
            }
        }

        public string ConnectionString
        {
            get { return connectionString; }
        }
    }
}
