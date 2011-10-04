using System.Collections;
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

		public void Insert(LogEventInfo item)
		{
			var db = _Server.GetDatabase(_Database);

			var collection = db.GetCollection<LogEventInfoData>(item.LoggerName);
			collection.Insert(new LogEventInfoData(item));
		}

		public void Dispose()
		{
			_Server.Disconnect();
			_Server = null;
		}
	}
}