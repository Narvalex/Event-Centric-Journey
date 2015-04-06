using Journey.Utils.Guids;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Journey.Tests.Guids
{
    public class SequentialGuidGeneratorFixture
    {
        protected readonly IGuidGenerator sut;

        public SequentialGuidGeneratorFixture()
        {
            this.sut = new SequentialGuid();
        }

        [Fact]
        public void WHEN_calling_generate_multiple_times_THEN_returns_unique_results()
        {
            int count = 1000;

            var results = new List<Guid>();

            for (int i = 0; i < count; i++)
            {
                var id = this.sut.NewGuid();
                results.Add(id);
            }

            Assert.Equal(count, results.Distinct().Count());   
        }

        [Fact]
        public void WHEN_executing_THEN_is_thread_safe()
        {
            var count = 1000;
            var list = new ConcurrentBag<Guid>();
            Parallel.For(0, count, i => list.Add(this.sut.NewGuid()));

            Assert.Equal(count, list.Distinct().Count());
        }
    }
}
