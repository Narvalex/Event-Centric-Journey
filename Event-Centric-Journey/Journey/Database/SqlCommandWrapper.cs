using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Journey.Database
{
    public class SqlCommandWrapper
    {
        protected readonly string connectionString;

        public SqlCommandWrapper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlCommandWrapper(IConnectionStringProvider connectionStringProvider)
        {
            this.connectionString = connectionStringProvider.ConnectionString;
        }

        public void CreateDatabase()
        {
            var builder = new SqlConnectionStringBuilder(this.connectionString);
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
                            @"
USE master
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
DROP DATABASE [{0}];
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') CREATE DATABASE [{0}];
",
                            databaseName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DropDatabase()
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
                            @"
USE master
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
DROP DATABASE [{0}];
",
                            databaseName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters.Count() > 0)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters.Count() > 0)
                    command.Parameters.AddRange(parameters);

                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }

        public IEnumerable<T> ExecuteReader<T>(string commandText, Func<IDataReader, T> proyect, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(this.connectionString);
            var command = new SqlCommand(commandText, connection);

            command.CommandType = CommandType.Text;
            if (parameters.Count() > 0)
                command.Parameters.AddRange(parameters);

            connection.Open();
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                {
                    yield return proyect(reader);
                }
            }
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(string commandText, Func<IDataReader, T> proyect, params SqlParameter[] parameters)
        {
            return await Task.Factory.StartNew<IEnumerable<T>>(() => this.ExecuteReader<T>(commandText, proyect, parameters), TaskCreationOptions.LongRunning);
        }
    }
}
