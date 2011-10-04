using MongoDB.Driver;

namespace NLog.MongoDB
{
	public interface IRepositoryProvider
	{
		IRepository GetRepository(MongoServerSettings setting,
		                          string database);
	}
}