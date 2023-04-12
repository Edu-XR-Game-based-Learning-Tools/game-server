using System;

namespace Core.Entity
{
    public class TMeta : BaseEntity, IAggregateRoot
    {
        public string MetaKey { get; private set; }
        public string MetaValue { get; set; }
        public string Description { get; set; }

        public TMeta(string metaKey, string metaValue)
        {
            MetaKey = !string.IsNullOrWhiteSpace(metaKey) ? metaKey : throw new ArgumentNullException(nameof(metaKey));
            MetaValue = metaValue;
        }
    }
}