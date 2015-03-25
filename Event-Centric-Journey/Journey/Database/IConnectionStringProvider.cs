namespace Infrastructure.CQRS.Database
{
    public interface IConnectionStringProvider
    {
        string ConnectionString { get; }
    }
}
