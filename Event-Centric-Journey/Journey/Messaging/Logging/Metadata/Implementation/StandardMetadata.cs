namespace Journey.Messaging.Logging.Metadata
{
    /// <summary>
    /// Exposes the property names of standard metadata added to all 
    /// messages going though the bus.
    /// </summary>
    public static class StandardMetadata
    {
        /// <summary>
        /// An event message.
        /// </summary>
        public const string EventKind = "Event";

        /// <summary>
        /// A command message.
        /// </summary>
        public const string CommandKind = "Command";

        /// <summary>
        /// Kind of message, either <see cref="EventKind"/> or <see cref="CommandKind"/>.
        /// </summary>
        public const string Kind = "Kind";

        /// <summary>
        /// Identifier of the object that originated the event, if any.
        /// </summary>
        public const string SourceId = "SourceId";

        /// <summary>
        /// Some messages, like Events, have version number.
        /// </summary>
        public const string Version = "Version";

        /// <summary>
        /// The simple assembly name of the message payload (i.e. event or command).
        /// </summary>
        public const string AssemblyName = "AssemblyName";

        /// <summary>
        /// The namespace of the message payload (i.e. event or command).
        /// </summary>
        public const string Namespace = "Namespace";

        /// <summary>
        /// The full type name of the message payload (i.e. event or command).
        /// </summary>
        public const string FullName = "FullName";

        /// <summary>
        /// The simple type name (without the namespace) of the message payload (i.e. event or command).
        /// </summary>
        public const string TypeName = "TypeName";

        /// <summary>
        /// The name of the entity that originated this message.
        /// </summary>
        public const string SourceType = "SourceType";

        public const string Origin = "Origin";

        public const string InternalOrigin = "Internal";

        public const string ExternalOrigin = "External";
    }
}
