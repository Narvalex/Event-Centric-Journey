namespace Journey.Messaging
{
    /// <summary>
    /// Revela para todos los implementadores los datos del schema en el cual 
    /// se implementa el sistema de mensajería
    /// </summary>
    public interface ISqlBus
    {
        /// <summary>
        /// The Table Name implemented for messaging capabilities
        /// </summary>
        string TableName { get; }
    }
}
