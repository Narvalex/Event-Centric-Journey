using Journey.EventSourcing;
using System;
using System.Runtime.Caching;
using Xunit;

namespace Journey.Tests.EventSourcing.SnapshotCacheFixture
{
    //public class GIVEN_empty_cache
    //{
    //    protected ISnapshotProvider sut;

    //    public GIVEN_empty_cache()
    //    {
    //        this.sut = new InMemoryRollingSnapshot("Test");
    //    }

    //    [Fact]
    //    public void THEN_the_cache_object_is_not_null()
    //    {
    //        Assert.NotNull(this.sut);
    //    }

    //}

    //public class WHEN_caching_objects : GIVEN_empty_cache
    //{
    //    [Fact]
    //    public void WHEN_caching_an_object_THEN_object_is_cached()
    //    {
    //        Assert.True(this.sut.GetCount() == 0);

    //        var payload = "Testing snapshotting";
    //        var id1 = 1;
    //        var aggregate = new FakeMementoAggregate(id1);
    //        aggregate.AddPayload(payload);

    //        var key = id1.ToString();
    //        var memento = ((IMementoOriginator)aggregate).SaveToMemento();
    //        this.sut.Set(
    //            key,
    //            new Tuple<IMemento, DateTime?>(memento, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30) });

    //        var cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 1);                        
    //    }

    //    [Fact]
    //    public void WHEN_caching_an_object_THEN_object_is_cached_AND_is_thread_safe()
    //    {
    //        Assert.True(this.sut.GetCount() == 0);

    //        var payload = "Testing snapshotting";
    //        var id1 = 1;
    //        var aggregate = new FakeMementoAggregate(id1);
    //        aggregate.AddPayload(payload);
    //        aggregate.Version = 1;

    //        var key = id1.ToString();
    //        var memento = ((IMementoOriginator)aggregate).SaveToMemento();
    //        this.sut.Set(
    //            key,
    //            new Tuple<IMemento, DateTime?>(memento, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddYears(1) });

    //        var cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 1);

    //        // Thread safety

    //        // Orignal Memento Object
    //        var originalMementoObject = (Tuple<IMemento, DateTime?>)this.sut.Get(id1.ToString());

    //        // updating values
    //        aggregate.AddPayload("Testing again. I'ts the 2nd time!");
    //        aggregate.Version = 2;
    //        memento = ((IMementoOriginator)aggregate).SaveToMemento();
    //        this.sut.Set(
    //            key,
    //            new Tuple<IMemento, DateTime?>(memento, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddYears(1) });

    //        cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 1);
    //        var updatedMementoObject = (Tuple<IMemento, DateTime?>)this.sut.Get(id1.ToString());
            
    //        // Checking the original
    //        Assert.Equal(1, originalMementoObject.Item1.Version);

    //        var originalPayload = (FakeMementoAggregate.Memento)originalMementoObject.Item1;
    //        Assert.Equal("Testing snapshotting", originalPayload.Payload);

    //        // Checking the updated
    //        Assert.Equal(2, updatedMementoObject.Item1.Version);

    //        var updatedPayload = (FakeMementoAggregate.Memento)updatedMementoObject.Item1;
    //        Assert.Equal("Testing again. I'ts the 2nd time!", updatedPayload.Payload);
    //    }

    //    [Fact]
    //    public void WHEN_an_object_cache_is_stale_THEN_it_is_disposed()
    //    {
    //        Assert.True(this.sut.GetCount() == 0);

    //        var payload = "Testing snapshotting";
    //        var id1 = 1;
    //        var aggregate = new FakeMementoAggregate(id1);
    //        aggregate.AddPayload(payload);

    //        var key = id1.ToString();
    //        var memento = ((IMementoOriginator)aggregate).SaveToMemento();
    //        this.sut.Set(
    //            key,
    //            new Tuple<IMemento, DateTime?>(memento, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(0) });

    //        var retrivedMementoObject = (Tuple<IMemento, DateTime?>)this.sut.Get(key);

    //        Assert.Null(retrivedMementoObject);
    //    }

    //    [Fact]
    //    public void WHEN_caching_a_batch_of_objects_THEN_all_are_cached()
    //    {
    //        Assert.True(this.sut.GetCount() == 0);

    //        var payload = "Testing snapshotting";
    //        var id1 = 1;
    //        var aggregate = new FakeMementoAggregate(id1);
    //        aggregate.AddPayload(payload);

    //        var key = id1.ToString();
    //        var memento = ((IMementoOriginator)aggregate).SaveToMemento();
    //        this.sut.Set(
    //            key,
    //            new Tuple<IMemento, DateTime?>(memento, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30) });

    //        var cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 1);


    //        var payload2 = "Testing snapshotting again";
    //        var id2 = 2;
    //        var aggregate2 = new FakeMementoAggregate(id2);
    //        aggregate2.AddPayload(payload2);

    //        var key2 = id2.ToString();
    //        var memento2 = ((IMementoOriginator)aggregate2).SaveToMemento();
    //        this.sut.Set(
    //            key2,
    //            new Tuple<IMemento, DateTime?>(memento, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30) });

    //        cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 2);
    //    }

    //    [Fact]
    //    public void WHEN_caching_a_batch_of_objects_THEN_can_retrieve_all()
    //    {
    //        Assert.True(this.sut.GetCount() == 0);

    //        var payload = "Testing snapshotting";
    //        var id1 = 1;
    //        var aggregate1 = new FakeMementoAggregate(id1);
    //        aggregate1.AddPayload(payload);

    //        var key1 = id1.ToString();
    //        var memento1 = ((IMementoOriginator)aggregate1).SaveToMemento();
    //        this.sut.Set(
    //            key1,
    //            new Tuple<IMemento, DateTime?>(memento1, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30) });

    //        var cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 1);


    //        var payload2 = "Testing snapshotting again";
    //        var id2 = 2;
    //        var aggregate2 = new FakeMementoAggregate(id2);
    //        aggregate2.AddPayload(payload2);

    //        var key2 = id2.ToString();
    //        var memento2 = ((IMementoOriginator)aggregate2).SaveToMemento();
    //        this.sut.Set(
    //            key2,
    //            new Tuple<IMemento, DateTime?>(memento2, DateTime.UtcNow),
    //            new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30) });

    //        cachedCount = this.sut.GetCount();
    //        Assert.True(cachedCount == 2);

    //        var rehydratedAggregate1 = new FakeMementoAggregate(id1, (Tuple<IMemento, DateTime?>)this.sut.Get(key1));
    //        var rehydratedAggregate2 = new FakeMementoAggregate(id2, (Tuple<IMemento, DateTime?>)this.sut.Get(key2));

    //        Assert.Equal(rehydratedAggregate1.Payload, aggregate1.Payload);
    //        Assert.Equal(rehydratedAggregate2.Payload, aggregate2.Payload);
    //    }
    //}

    //public class FakeMementoAggregate : IMementoOriginator
    //{
    //    public FakeMementoAggregate(int id)
    //    {
    //        this.Id = id;
    //    }

    //    public FakeMementoAggregate(int id, Tuple<IMemento, DateTime?> memento)
    //        : this(id)
    //    {
    //        var cached = (Memento)memento.Item1;
    //        this.Payload = cached.Payload;
    //        this.Version = cached.Version;
    //    }

    //    public int Version { get; set; }
    //    public int Id { get; private set; }
    //    public string Payload { get; set; }
    //    public void AddPayload(string payload)
    //    {
    //        this.Payload = payload;
    //    }
    //    public IMemento SaveToMemento()
    //    {
    //        return new Memento
    //        {
    //            Version = this.Version,
    //            Payload = this.Payload
    //        };
    //    }

    //    public class Memento : IMemento
    //    {
    //        public int Version { get; internal set; }
    //        public string Payload { get; set; }
    //    }
    //}
}
