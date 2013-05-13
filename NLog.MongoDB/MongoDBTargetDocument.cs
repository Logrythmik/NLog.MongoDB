using System.Collections.Generic;
using NLog.Config;
using NLog.Layouts;

namespace NLog.MongoDB
{
    [NLogConfigurationItem]
    public sealed class MongoDBTargetDocument
    {
        public MongoDBTargetDocument()
        {
            Fields = new List<MongoDBTargetField>();
            Documents = new List<MongoDBTargetDocument>();
        }

        [RequiredParameter]
        public string Name { get; set; }

        [ArrayParameter(typeof(MongoDBTargetField), "field")]
        public IList<MongoDBTargetField> Fields { get; private set; }

        [ArrayParameter(typeof(MongoDBTargetDocument), "document")]
        public IList<MongoDBTargetDocument> Documents { get; private set; }
    }
}
