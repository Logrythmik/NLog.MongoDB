using MongoDB.Driver;

namespace NLog.MongoDB
{
	public class MongoServerProvider : IRepositoryProvider
	{
        public IRepository GetRepository(MongoServerSettings settings, string databaseName)
	    {
	        return new MongoRepository(settings, databaseName);
	    }
	}
}