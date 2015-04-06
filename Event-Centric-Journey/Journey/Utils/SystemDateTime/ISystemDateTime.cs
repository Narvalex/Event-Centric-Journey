using System;
namespace Journey.Utils.SystemDateTime
{
    /// <summary>
    /// Define una unica forma de generar la fecha y la hora. Si va a ser
    /// UTC o la hora y fecha locales.
    /// </summary>
    public interface ISystemDateTime
    {
        DateTime Now { get; }
    }
}
