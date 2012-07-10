using System.Configuration;

namespace NLog.MongoDB
{
	public sealed class MongoRepositoryProvider : IRepositoryProvider
	{
		public IRepository GetRepository(string connectionName)
		{
			var connectionString = GetConnectionString(connectionName);

			return new MongoRepository(connectionString);
		}

		private string GetConnectionString(string connectionName)
		{
			return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
		}
	}
}