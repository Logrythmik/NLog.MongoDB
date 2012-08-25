using MongoDB.Driver;

namespace NLog.MongoDB
{
	public interface IRepositoryProvider
	{
        IRepository GetRepository(MongoServerSettings settings, string databaseName);
	}
}