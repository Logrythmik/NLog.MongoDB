using MongoDB.Driver;

namespace NLog.MongoDB
{
	public class MongoRepository : IRepository
	{
		private MongoDatabase _database;

		public MongoRepository(string connectionString)
		{
			_database = MongoDatabase.Create(connectionString);
		}

		public void Insert(LogEventInfo item)
		{
			var collection = _database.GetCollection<LogEventInfoData>(item.LoggerName);
			
			collection.Insert(new LogEventInfoData(item));
		}
	}
}