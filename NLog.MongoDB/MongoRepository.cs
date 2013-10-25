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

		private static readonly Dictionary<string, bool> _collectionCache = new Dictionary<string, bool>();

        public MongoRepository(MongoServerSettings settings, string databaseName)
	    {
            _server = new MongoServer(settings);
            _database = databaseName;
	        _server.Connect();
        }

        public void CheckCollection(string collectionName, long collectionSize, long? collectionMaxItems, bool createIdField)
        {
            var db = _server.GetDatabase(_database);

            lock (_collectionCache)
            {
                if (_collectionCache.ContainsKey(collectionName)) return;
                
                if (!db.CollectionExists(collectionName))
                {
                    var collectionOptionsBuilder = new CollectionOptionsBuilder();

					collectionOptionsBuilder.SetCapped(true);
					collectionOptionsBuilder.SetMaxSize(collectionSize);
					collectionOptionsBuilder.SetAutoIndexId(createIdField);

                    if (collectionMaxItems.HasValue)
						collectionOptionsBuilder.SetMaxDocuments(collectionMaxItems.Value);

	                db.CreateCollection(collectionName, collectionOptionsBuilder);

                }

                _collectionCache.Add(collectionName, createIdField);
            }

        }

	    public void Insert(string collectionName, BsonDocument item)
		{
			var db = _server.GetDatabase(_database);

		    MongoCollection collection;

			// if we shouldn't save ids
		    if (_collectionCache.ContainsKey(collectionName) && !_collectionCache[collectionName])
			    collection = db.GetCollection(collectionName, new MongoCollectionSettings {AssignIdOnInsert = false});
			else
				collection = db.GetCollection(collectionName);
			
			collection.Insert(item);
		}

		public void Dispose()
		{
			_server.Disconnect();
			_server = null;
		}
	}
}