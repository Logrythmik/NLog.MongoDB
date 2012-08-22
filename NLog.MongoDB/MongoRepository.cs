using MongoDB.Bson;
using MongoDB.Driver;

namespace NLog.MongoDB
{
    using System.Collections.Generic;
    using global::MongoDB.Driver.Builders;

    public class MongoRepository : IRepository
	{
		private MongoServer _Server;
		private readonly string _Database;
        private static readonly List<string> CollectionCache = new List<string>();

        public MongoRepository(MongoServerSettings settings, string databaseName)
	    {
            _Server = new MongoServer(settings);
            _Database = databaseName;
	        _Server.Connect();
        }

        public void CheckCollection(string collectionName, bool useCappedCollection, long? cappedCollectionSize, long? cappedCollectionMaxItems, bool createIdField)
        {
            if (useCappedCollection && !cappedCollectionSize.HasValue)
            {
                throw new NLogConfigurationException("If UseCappedCollection is set to true, you must set CappedCollectionSize!");
            }

            var db = _Server.GetDatabase(_Database);

            lock (CollectionCache)
            {
                if (!useCappedCollection || CollectionCache.Contains(collectionName)) return;
                
                if (!db.CollectionExists(collectionName))
                {
                    var collectionOptionsBuilder = new CollectionOptionsBuilder();
                    collectionOptionsBuilder.SetCapped(true);
                        
                    if (createIdField) collectionOptionsBuilder.SetAutoIndexId(true);
                    if (cappedCollectionSize.HasValue) collectionOptionsBuilder.SetMaxSize(cappedCollectionSize.Value);
                    if (cappedCollectionMaxItems.HasValue) collectionOptionsBuilder.SetMaxDocuments(cappedCollectionMaxItems.Value);

                    db.CreateCollection(collectionName, collectionOptionsBuilder);
                }

                CollectionCache.Add(collectionName);
            }

        }

	    public void Insert(string collectionName, BsonDocument item)
		{
			var db = _Server.GetDatabase(_Database);
	        var collection = db.GetCollection(collectionName);
			collection.Insert(item);
		}

		public void Dispose()
		{
			_Server.Disconnect();
			_Server = null;
		}
	}
}