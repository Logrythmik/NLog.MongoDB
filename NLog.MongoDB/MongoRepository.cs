using MongoDB.Bson;
using MongoDB.Driver;

namespace NLog.MongoDB
{
    using System.Collections.Generic;
    using global::MongoDB.Driver.Builders;

	internal sealed class MongoRepository : IRepository
	{
		private MongoServer _server;
		private readonly string _database;

		private static readonly List<string> CollectionCache = new List<string>();

        public MongoRepository(MongoServerSettings settings, string databaseName)
	    {
            _server = new MongoServer(settings);
            _database = databaseName;
	        _server.Connect();
        }

        public void CheckCollection(string collectionName, long collectionSize, long? collectionMaxItems, bool createIdField)
        {
            var db = _server.GetDatabase(_database);

            lock (CollectionCache)
            {
                if (CollectionCache.Contains(collectionName)) return;
                
                if (!db.CollectionExists(collectionName))
                {
                    var collectionOptionsBuilder = new CollectionOptionsBuilder();

					collectionOptionsBuilder.SetCapped(true);
					collectionOptionsBuilder.SetMaxSize(collectionSize);

                    if (createIdField)
						collectionOptionsBuilder.SetAutoIndexId(true);
                    
					if (collectionMaxItems.HasValue)
						collectionOptionsBuilder.SetMaxDocuments(collectionMaxItems.Value);

                    db.CreateCollection(collectionName, collectionOptionsBuilder);
                }

                CollectionCache.Add(collectionName);
            }

        }

	    public void Insert(string collectionName, BsonDocument item)
		{
			var db = _server.GetDatabase(_database);
	        var collection = db.GetCollection(collectionName);
			collection.Insert(item);
		}

		public void Dispose()
		{
			_server.Disconnect();
			_server = null;
		}
	}
}