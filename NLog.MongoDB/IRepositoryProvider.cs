using MongoDB.Driver;

namespace NLog.MongoDB
{
	public interface IRepositoryProvider
	{
		IRepository GetRepository(MongoServerSettings settings, string database);
        IRepository GetRepository(string connectionString, string database);
	}
}