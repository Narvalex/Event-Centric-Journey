using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Infrastructure.CQRS.Messaging
{
    /// <summary>
    /// This database initializer is to support <see cref="CommandBus"/> and <see cref="EventBus"/>.
    /// </summary>
    public class MessagingDbInitializer
    {
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Does not contain user input.")]
        public static void CreateDatabaseObjects(string connectionString, string schema, bool createDatabase = false)
        {
            if (createDatabase)
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";
                builder.AttachDBFilename = string.Empty;

                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText =
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') CREATE DATABASE [{0}];",
                                databaseName);

                        command.ExecuteNonQuery();
                    }
                }
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{0}')
EXECUTE sp_executesql N'CREATE SCHEMA [{0}] AUTHORIZATION [dbo]';
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[Commands]') AND type in (N'U'))
CREATE TABLE [{0}].[Commands](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [Body] [nvarchar](max) NOT NULL,
    [DeliveryDate] [datetime] NULL,
    [CorrelationId] [nvarchar](max) NULL,
	[IsDeadLetter] [bit] NOT NULL,
	[TraceInfo][nvarchar](max) NULL
 CONSTRAINT [PK_{0}.Commands] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[Events]') AND type in (N'U'))
CREATE TABLE [{0}].[Events](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [Body] [nvarchar](max) NOT NULL,
    [DeliveryDate] [datetime] NULL,
    [CorrelationId] [nvarchar](max) NULL,
	[IsDeadLetter] [bit] NOT NULL,
	[TraceInfo][nvarchar](max) NULL
 CONSTRAINT [PK_{0}.Events] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY];
",
                            schema);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
