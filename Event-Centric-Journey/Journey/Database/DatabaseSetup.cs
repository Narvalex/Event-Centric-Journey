using System.Data.Entity;

namespace Journey.Database
{
    public static class DatabaseSetup
    {
        public static void Initialize()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            System.Data.Entity.Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(System.Data.Entity.Database.DefaultConnectionFactory);
        }
    }
}
