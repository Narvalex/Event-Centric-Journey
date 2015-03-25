using System;
namespace Journey.Messaging
{
    public interface IMessagingSettings
    {
        int NumberOfThreads { get; }

        TimeSpan BusPollDelay { get; }
    }

    public class MessagingSettings : IMessagingSettings
    {
        public MessagingSettings(int numberOfThreads, TimeSpan busPollDelay)
        {
            this.NumberOfThreads = numberOfThreads;
            this.BusPollDelay = busPollDelay;
        }

        public int NumberOfThreads { get; private set; }

        public TimeSpan BusPollDelay { get; private set; }
    }
}
