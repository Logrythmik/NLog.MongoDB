using MongoDB.Driver;

namespace NLog.MongoDB20
{
	public interface IRepositoryProvider
	{
        IRepository GetRepository(MongoClientSettings settings, string databaseName);
	}
}