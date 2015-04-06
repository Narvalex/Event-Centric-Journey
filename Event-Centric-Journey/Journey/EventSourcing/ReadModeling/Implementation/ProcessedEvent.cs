using System;

namespace Journey.EventSourcing.ReadModeling
{
    /// <summary>
    /// Un evento que ya fue procesado
    /// </summary>
    public class ProcessedEvent
    {
        /// <summary>
        /// El id del aggregate, o Actor. Esto junto con la versión se 
        /// utiliza para saber si el evento ya ha sido procesado por el 
        /// read model generator.
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// El tipo de actor o aggrefate. Esto es un metadata que
        /// ayuda a que sea trazable.
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// La versión del actor o aggregate. Esto junto al aggregate id sirve para 
        /// ayudar al read model genertor para saber si el evento ya se procesó.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Just a Metadata
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// El id del comando que correlacionado con el evento 
        /// que materializa un read model. Es que generó el command 
        /// luego viene a buscar este id para notificar al usuario que 
        /// se ha procesado su comando.
        /// </summary>
        public Guid CorrelationId { get; set; }
    }
}
