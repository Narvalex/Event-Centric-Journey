namespace Journey.EventSourcing
{
    public interface IRehydratesFrom { }

    public interface IRehydratesFrom<T> : IRehydratesFrom
        where T : IVersionedEvent
    {
        void Rehydrate(T e);
    }
}
