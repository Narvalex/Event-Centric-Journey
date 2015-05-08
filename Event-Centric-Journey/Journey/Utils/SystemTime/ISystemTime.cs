using System;

namespace Journey.Utils.SystemTime
{
    /// <summary>
    /// Define una unica forma de generar la fecha y la hora. Si va a ser
    /// UTC o la hora y fecha locales.
    /// </summary>
    public interface ISystemTime
    {
        DateTime Now { get; }

        DateTimeOffset NowOffset { get; }
    }
}
