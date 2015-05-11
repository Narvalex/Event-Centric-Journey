using Journey.Worker;
using Xunit;

namespace Journey.Tests.Instrumentation.ConsoleWorkerRoleTracerFixture
{
    public class GIVEN_tracer
    {
        private ConsoleTracer sut;

        public GIVEN_tracer()
        {
            this.sut = new ConsoleTracer();
        }

        [Fact]
        public void WHEN_something_normal_occurs_THEN_trace_info()
        {
            this.sut.TraceAsync("algo normal ocurrio... nada que mirar aqui.");
        }
    }
}
