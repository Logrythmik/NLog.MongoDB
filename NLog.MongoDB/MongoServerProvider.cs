using MongoDB.Driver;

namespace NLog.MongoDB
{
	public class MongoServerProvider : IRepositoryProvider
	{
		public IRepository GetRepository(
			MongoServerSettings settings,
			string database)
		{
			return new MongoRepository(settings, database);
		}
	}
}