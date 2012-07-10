namespace NLog.MongoDB
{
	public interface IRepositoryProvider
	{
		IRepository GetRepository(string connectionName);
	}
}