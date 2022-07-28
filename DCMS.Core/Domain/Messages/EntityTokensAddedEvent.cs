using System.Collections.Generic;

namespace DCMS.Core.Domain.Messages
{

    public class EntityTokensAddedEvent<T, U> where T : BaseEntity
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="tokens">Tokens</param>
        public EntityTokensAddedEvent(T entity, IList<U> tokens)
        {
            Entity = entity;
            Tokens = tokens;
        }

        /// <summary>
        /// Entity
        /// </summary>
        public T Entity { get; }

        /// <summary>
        /// Tokens
        /// </summary>
        public IList<U> Tokens { get; }
    }
}