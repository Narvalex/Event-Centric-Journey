using Journey.Worker;
using Xunit;

namespace Journey.Tests.Instrumentation.ConsoleWorkerRoleTracerFixture
{
    public class GIVEN_tracer
    {
        private ConsoleWorkerRoleTracer sut;

        public GIVEN_tracer()
        {
            this.sut = new ConsoleWorkerRoleTracer();
        }

        [Fact]
        public void WHEN_something_normal_occurs_THEN_trace_info()
        {
            this.sut.Notify("algo normal ocurrio... nada que mirar aqui.");
        }
    }
}
