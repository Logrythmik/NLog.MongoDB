using MongoDB.Driver;

namespace NLog.MongoDB20
{
	public class MongoServerProvider : IRepositoryProvider
	{
        public IRepository GetRepository(MongoClientSettings settings, string databaseName)
	    {
	        return new MongoRepository(settings, databaseName);
	    }
	}
}