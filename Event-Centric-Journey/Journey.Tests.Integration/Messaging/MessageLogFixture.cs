using Journey.EventSourcing;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils.SystemDateTime;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Journey.Tests.Integration.Messaging.MessageLogFixture
{
    public class GIVEN_a_sql_log_with_three_events : IDisposable
    {
        private string dbName = "EventLogFixture";
        private MessageLog sut;
        private Mock<IMetadataProvider> metadata;
        private EventA eventA;
        private EventB eventB;
        private EventC eventC;

        public GIVEN_a_sql_log_with_three_events()
        {
            using (var context = new MessageLogDbContext(dbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            this.eventA = new EventA();
            this.eventB = new EventB();
            this.eventC = new EventC();

            var metadata = Mock.Of<IMetadataProvider>(p =>
                p.GetMetadata(eventA) == new Dictionary<string, string>
                {
                    { StandardMetadata.SourceId, eventA.SourceId.ToString() },
                    { StandardMetadata.SourceType, "SourceA" }, 
                    { StandardMetadata.Kind, StandardMetadata.EventKind },
                    { StandardMetadata.AssemblyName, "A" }, 
                    { StandardMetadata.Namespace, "Namespace" }, 
                    { StandardMetadata.FullName, "Namespace.EventA" }, 
                    { StandardMetadata.TypeName, "EventA" }, 
                } &&
                p.GetMetadata(eventB) == new Dictionary<string, string>
                {
                    { StandardMetadata.SourceId, eventB.SourceId.ToString() },
                    { StandardMetadata.SourceType, "SourceB" }, 
                    { StandardMetadata.Kind, StandardMetadata.EventKind },
                    { StandardMetadata.AssemblyName, "B" }, 
                    { StandardMetadata.Namespace, "Namespace" }, 
                    { StandardMetadata.FullName, "Namespace.EventB" }, 
                    { StandardMetadata.TypeName, "EventB" }, 
                } &&
                p.GetMetadata(eventC) == new Dictionary<string, string>
                {
                    { StandardMetadata.SourceId, eventC.SourceId.ToString() },
                    { StandardMetadata.SourceType, "SourceC" }, 
                    { StandardMetadata.Kind, StandardMetadata.EventKind },
                    { StandardMetadata.AssemblyName, "B" }, 
                    { StandardMetadata.Namespace, "AnotherNamespace" }, 
                    { StandardMetadata.FullName, "AnotherNamespace.EventC" }, 
                    { StandardMetadata.TypeName, "EventC" }, 
                });

            this.metadata = Mock.Get(metadata);
            this.sut = new MessageLog(this.dbName, new IndentedJsonTextSerializer(), metadata, new LocalDateTime());
            this.sut.Save(eventA);
            this.sut.Save(eventB);
            this.sut.Save(eventC);
        }

        [Fact]
        public void THEN_can_read_all()
        {
            var events = this.sut.ReadAll().ToList();

            Assert.Equal(3, events.Count);
        }

        [Fact]
        public void THEN_can_filter_by_assembly()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { AssemblyNames = { "A" } }).ToList();

            Assert.Equal(1, events.Count);
        }

        [Fact]
        public void then_can_filter_by_multiple_assemblies()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { AssemblyNames = { "A", "B" } }).ToList();

            Assert.Equal(3, events.Count);
        }

        [Fact]
        public void then_can_filter_by_namespace()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { Namespaces = { "Namespace" } }).ToList();

            Assert.Equal(2, events.Count);
            Assert.True(events.Any(x => x.SourceId == eventA.SourceId));
            Assert.True(events.Any(x => x.SourceId == eventB.SourceId));
        }

        [Fact]
        public void then_can_filter_by_namespaces()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { Namespaces = { "Namespace", "AnotherNamespace" } }).ToList();

            Assert.Equal(3, events.Count);
        }

        [Fact]
        public void then_can_filter_by_namespace_and_assembly()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { AssemblyNames = { "B" }, Namespaces = { "AnotherNamespace" } }).ToList();

            Assert.Equal(1, events.Count);
            Assert.True(events.Any(x => x.SourceId == eventC.SourceId));
        }

        [Fact]
        public void then_can_filter_by_namespace_and_assembly2()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { AssemblyNames = { "A" }, Namespaces = { "AnotherNamespace" } }).ToList();

            Assert.Equal(0, events.Count);
        }

        [Fact]
        public void then_can_filter_by_full_name()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { FullNames = { "Namespace.EventA" } }).ToList();

            Assert.Equal(1, events.Count);
            Assert.Equal(eventA.SourceId, events[0].SourceId);
        }

        [Fact]
        public void then_can_filter_by_full_names()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { FullNames = { "Namespace.EventA", "AnotherNamespace.EventC" } }).ToList();

            Assert.Equal(2, events.Count);
            Assert.True(events.Any(x => x.SourceId == eventA.SourceId));
            Assert.True(events.Any(x => x.SourceId == eventC.SourceId));
        }

        [Fact]
        public void then_can_filter_by_type_name()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { TypeNames = { "EventA" } }).ToList();

            Assert.Equal(1, events.Count);
            Assert.Equal(eventA.SourceId, events[0].SourceId);
        }

        [Fact]
        public void then_can_filter_by_type_names()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { TypeNames = { "EventA", "EventC" } }).ToList();

            Assert.Equal(2, events.Count);
            Assert.True(events.Any(x => x.SourceId == eventA.SourceId));
            Assert.True(events.Any(x => x.SourceId == eventC.SourceId));
        }

        [Fact]
        public void then_can_filter_by_type_names_and_assembly()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { AssemblyNames = { "B" }, TypeNames = { "EventB", "EventC" } }).ToList();

            Assert.Equal(2, events.Count);
            Assert.True(events.Any(x => x.SourceId == eventB.SourceId));
            Assert.True(events.Any(x => x.SourceId == eventC.SourceId));
        }

        [Fact]
        public void then_can_filter_by_source_id()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { SourceIds = { eventA.SourceId.ToString() } }).ToList();

            Assert.Equal(1, events.Count);
            Assert.Equal(eventA.SourceId, events[0].SourceId);
        }

        [Fact]
        public void then_can_filter_by_source_ids()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { SourceIds = { eventA.SourceId.ToString(), eventC.SourceId.ToString() } }).ToList();

            Assert.Equal(2, events.Count);
            Assert.True(events.Any(x => x.SourceId == eventA.SourceId));
            Assert.True(events.Any(x => x.SourceId == eventC.SourceId));
        }

        [Fact]
        public void then_can_filter_by_source_type()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { SourceTypes = { "SourceA" } }).ToList();

            Assert.Equal(1, events.Count);
        }

        [Fact]
        public void then_can_filter_by_source_types()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { SourceTypes = { "SourceA", "SourceB" } }).ToList();

            Assert.Equal(2, events.Count);
        }

        [Fact]
        public void then_can_filter_in_by_end_date()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { EndDate = DateTime.UtcNow }).ToList();

            Assert.Equal(3, events.Count);
        }

        [Fact]
        public void then_can_filter_out_by_end_date()
        {
            var events = this.sut.Query(new EventLogQueryCriteria { EndDate = DateTime.Now.AddMinutes(-1) }).ToList();

            Assert.Equal(0, events.Count);
        }

        [Fact]
        public void then_can_use_fluent_criteria_builder()
        {
            var events = this.sut.Query()
                .FromAssembly("A")
                .FromAssembly("B")
                .FromNamespace("Namespace")
                .FromSource("SourceB")
                .WithTypeName("EventB")
                .WithFullName("Namespace.EventB")
                .Until(DateTime.UtcNow)
                .ToList();

            Assert.Equal(1, events.Count);
        }

        public class EventA : VersionedEvent
        {
            public EventA()
            {
                this.SourceId = Guid.NewGuid();
            }            
        }

        public void Dispose()
        {
            var connectionString = string.Empty;
            using (var context = new MessageLogDbContext(dbName))
            {
                connectionString = context.Database.Connection.ConnectionString;
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
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
ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
DROP DATABASE [{0}]
",
                            this.dbName);

                    command.ExecuteNonQuery();
                }
            }


        }

        public class EventB : VersionedEvent
        {
            public EventB()
            {
                this.SourceId = Guid.NewGuid();
            }
        }

        public class EventC : VersionedEvent
        {
            public EventC()
            {
                this.SourceId = Guid.NewGuid();
            }
            
        }
    }

    public class GIVEN_a_sql_log_with_commands : IDisposable
    {
        private string dbName = "EventLogFixture";
        private MessageLog sut;
        private Mock<IMetadataProvider> metadata;
        private EventA eventA;
        private EventB eventB;

        public GIVEN_a_sql_log_with_commands()
        {
            using (var context = new MessageLogDbContext(dbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            this.eventA = new EventA();
            this.eventB = new EventB();

            var metadata = Mock.Of<IMetadataProvider>(p =>
                p.GetMetadata(eventA) == new Dictionary<string, string>
                {
                    { StandardMetadata.SourceId, eventA.SourceId.ToString() },
                    { StandardMetadata.SourceType, "SourceA" }, 
                    { StandardMetadata.Kind, StandardMetadata.EventKind },
                    { StandardMetadata.AssemblyName, "A" }, 
                    { StandardMetadata.Namespace, "Namespace" }, 
                    { StandardMetadata.FullName, "Namespace.EventA" }, 
                    { StandardMetadata.TypeName, "EventA" }, 
                } &&
                p.GetMetadata(eventB) == new Dictionary<string, string>
                {
                    { StandardMetadata.SourceId, eventB.SourceId.ToString() },
                    { StandardMetadata.SourceType, "SourceB" }, 
                    { StandardMetadata.Kind, StandardMetadata.EventKind },
                    { StandardMetadata.AssemblyName, "B" }, 
                    { StandardMetadata.Namespace, "Namespace" }, 
                    { StandardMetadata.FullName, "Namespace.EventB" }, 
                    { StandardMetadata.TypeName, "EventB" }, 
                });

            this.metadata = Mock.Get(metadata);
            this.sut = new MessageLog(this.dbName, new IndentedJsonTextSerializer(), metadata, new LocalDateTime());
            this.sut.Save(eventA);
            this.sut.Save(eventB);
        }

        [Fact]
        public void THEN_can_read_all()
        {
            var events = this.sut.ReadAll().ToList();

            Assert.Equal(2, events.Count);
        }

        public class EventA : VersionedEvent
        {
            public EventA()
            {
                this.SourceId = Guid.NewGuid();
            }            
        }

        public void Dispose()
        {
            var connectionString = string.Empty;
            using (var context = new MessageLogDbContext(dbName))
            {
                connectionString = context.Database.Connection.ConnectionString;
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
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
ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
DROP DATABASE [{0}]
",
                            this.dbName);

                    command.ExecuteNonQuery();
                }
            }


        }

        public class EventB : VersionedEvent
        {
            public EventB()
            {
                this.SourceId = Guid.NewGuid();
            }
        }
    }
}
