using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NLog.MongoDB
{
	public class MongoRepository : IRepository
	{
		private MongoServer _Server;
		private readonly string _Database;

		public MongoRepository(
			MongoServerSettings settings,
			string database)
		{
			_Server = new MongoServer(settings);
            _Database = database;
			_Server.Connect();
		}

        public MongoRepository(
            string connectionString,
            string database)
        {
            _Server = MongoServer.Create(connectionString);
            _Database = connectionString.ParseDatabaseName() ?? database;
            _Server.Connect();
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