namespace Journey.EventSourcing.Handling
{
    /// <summary>
    /// Un Domain Service que es idempotente. No debe depender de factores externos, solamente a un sistema interno,
    /// quizás algún cálculo, pero no debe obtener datos de afuera que puden cambiar. Esto garantiza el command sourcing.
    /// </summary>
    public interface IDomainService
    { }
}
