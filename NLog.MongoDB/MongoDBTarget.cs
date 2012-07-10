using NLog.Targets;

namespace NLog.MongoDB
{
	[Target("MongoDB")]
	public sealed class MongoDBTarget : Target
	{
		private IRepositoryProvider _repositoryProvider;

		public MongoDBTarget() : this(new MongoRepositoryProvider())
		{
		}

		public MongoDBTarget(IRepositoryProvider repositoryProvider)
		{
			_repositoryProvider = repositoryProvider;
		}

		public string ConnectionName { get; set; }

		internal void TestWrite(LogEventInfo logEvent)
		{
			Write(logEvent);
		}

		protected override void Write(LogEventInfo logEvent)
		{
			var repository = _repositoryProvider.GetRepository(ConnectionName);

			repository.Insert(logEvent);
		}
	}
}
