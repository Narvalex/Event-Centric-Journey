using Journey.Database;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Journey.Tests.Integration.Utils.Database.SqlCommandWrapperFixture
{
    public class GIVEN_connectionString
    {
        protected readonly string connectionString;
        private readonly string dbName = "SqlCommandWrapperFixture";

        protected SqlCommandWrapper sut;

        public GIVEN_connectionString()
        {
            var connectionFactory = System.Data.Entity.Database.DefaultConnectionFactory;
            this.connectionString = connectionFactory.CreateConnection(this.dbName).ConnectionString;

            this.sut = new SqlCommandWrapper(this.connectionString);
        }

        [Fact]
        private void THEN_can_create_and_drop_database()
        {
            this.sut.CreateDatabase();
            this.sut.DropDatabase();
        }
    }

    public class GIVEN_db : GIVEN_connectionString, IDisposable
    {
        public GIVEN_db()
        {
            this.sut.CreateDatabase();
        }

        public void Dispose()
        {
            this.sut.DropDatabase();
        }

        [Fact]
        private async Task THEN_can_execute_query_without_parameters()
        {
            await this.sut.ExecuteNonQueryAsync(@"
CREATE TABLE [dbo].[SIClientesPedidosOnlineView](
	[IdCliente] [int] NOT NULL,
	[Email] [nvarchar](50) NULL
 CONSTRAINT [PK_SIClientesPedidosOnline] PRIMARY KEY CLUSTERED 
(
	[IdCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
");
        }

        [Fact]
        private async Task THEN_can_execute_query_with_parameters()
        {
            await this.sut.ExecuteNonQueryAsync(@"
CREATE TABLE [dbo].[SIClientesPedidosOnlineView](
	[IdCliente] [int] NOT NULL,
	[Email] [nvarchar](50) NULL
 CONSTRAINT [PK_SIClientesPedidosOnline] PRIMARY KEY CLUSTERED 
(
	[IdCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
");
            await this.sut.ExecuteNonQueryAsync(@"
INSERT INTO dbo.SIClientesPedidosOnlineView
(
	[IdCliente],
	[Email]
)
VALUES
(
    @IdCliente,
    @Email
)
",
            new SqlParameter("@IdCliente", 1), new SqlParameter("@Email", "test@hotmail.com"));
        }
    }

    public class GIVEN_table_with_records : GIVEN_db, IDisposable
    {
        public GIVEN_table_with_records()
        {
            this.sut.ExecuteNonQueryAsync(@"
CREATE TABLE [dbo].[SIClientesPedidosOnlineView](
	[IdCliente] [int] NOT NULL,
	[Email] [nvarchar](50) NULL
 CONSTRAINT [PK_SIClientesPedidosOnline] PRIMARY KEY CLUSTERED 
(
	[IdCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
").Wait();

        }

        [Fact]
        private void GIVEN_no_rows_WHEN_querying_without_parameters_THEN_can_return_list()
        {
            var list = this.sut.ExecuteReader<Cliente>(@"
SELECT  [IdCliente],
	    [Email]
FROM dbo.SIClientesPedidosOnlineView
",
                        (r) =>
                        {
                            return new Cliente
                            {
                                IdCliente = r.SafeGetInt32(0),
                                Email = r.SafeGetString(1)
                            };
                        }).ToList();



            Assert.Empty(list);
            Assert.Equal(0, list.Count());
        }

        [Fact]
        private async Task GIVEN_no_rows_WHEN_querying_without_parameters_THEN_can_return_list_async()
        {
            var query = await this.sut.ExecuteReaderAsync<Cliente>(@"
SELECT  [IdCliente],
	    [Email]
FROM dbo.SIClientesPedidosOnlineView
",
                        (r) =>
                        {
                            return new Cliente
                            {
                                IdCliente = r.SafeGetInt32(0),
                                Email = r.SafeGetString(1)
                            };
                        });


            var list = query.ToList();
            Assert.Empty(list);
            Assert.Equal(0, list.Count());
        }

        [Fact]
        private void GIVEN_rows_WHEN_querying_without_parameters_THEN_can_return_list()
        {
            this.InsertRows();

            var list = this.sut.ExecuteReader<Cliente>(@"
SELECT  [IdCliente],
	    [Email]
FROM dbo.SIClientesPedidosOnlineView
",
                        (r) =>
                        {
                            return new Cliente
                            {
                                IdCliente = r.SafeGetInt32(0),
                                Email = r.SafeGetString(1)
                            };
                        }).ToList();
                        


            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        private async Task GIVEN_rows_WHEN_querying_without_parameters_THEN_can_return_list_async()
        {
            this.InsertRows();

            var query = await this.sut.ExecuteReaderAsync<Cliente>(@"
SELECT  [IdCliente],
	    [Email]
FROM dbo.SIClientesPedidosOnlineView
",
                        (r) =>
                        {
                            return new Cliente
                            {
                                IdCliente = r.SafeGetInt32(0),
                                Email = r.SafeGetString(1)
                            };
                        });

            var list = query.ToList();

            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        private async Task GIVEN_rows_WHEN_querying_with_parameters_THEN_can_return_int_async()
        {
            this.InsertRows();

            var query = await this.sut.ExecuteReaderAsync<int>(@"
SELECT  [IdCliente]
FROM dbo.SIClientesPedidosOnlineView
WHERE Email = @Email
",
                        (r) =>
                        {
                            return r.SafeGetInt32(0);
                        },
                        new SqlParameter("@Email", "test@hotmail.com"));

            var scalar = query.FirstOrDefault();

            Assert.Equal(1, scalar);
        }

        private void InsertRows()
        {
            this.sut.ExecuteNonQueryAsync(@"
INSERT INTO dbo.SIClientesPedidosOnlineView
(
	[IdCliente],
	[Email]
)
VALUES
(
    @IdCliente,
    @Email
)
",
new SqlParameter("@IdCliente", 1), new SqlParameter("@Email", "test@hotmail.com")).Wait();

            this.sut.ExecuteNonQueryAsync(@"
INSERT INTO dbo.SIClientesPedidosOnlineView
(
	[IdCliente],
	[Email]
)
VALUES
(
    @IdCliente,
    @Email
)
",
new SqlParameter("@IdCliente", 2), new SqlParameter("@Email", "amigoDeTest@hotmail.com")).Wait();
        }

        private class Cliente
        {
            public int IdCliente { get; set; }
            public string Email { get; set; }
        }
    }
}
