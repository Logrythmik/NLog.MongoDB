using System;
using System.Configuration;
using MongoDB.Driver;
using NLog.Targets;

namespace NLog.MongoDB
{
	[Target("MongoDB")]
	public sealed class MongoDBTarget : Target
	{
		public Func<IRepositoryProvider> Provider = () => new MongoServerProvider();

        #region Exposed Properties

        public string ConnectionString { get; set; }

        public string ConnectionName { get; set; }
		
		public string Host
		{
			get { return _Host ?? "localhost"; }
			set { _Host = value; }
		}
		private string _Host;

		public int Port	
		{
			get { return _Port ?? 27017; }
			set { _Port = value; }
		}
		private int? _Port;

        public string Username { get; set; }

        public string Password { get; set; }

		public string Database
		{
			get { return _Database ?? "NLog"; }
			set { _Database = value; }
		}
		private string _Database;

        #endregion

        #region Private Helpers

        private IRepository GetRepository()
        {
            // We have a connection string name, grab this from the config and pass it too the parser.
            if (!string.IsNullOrWhiteSpace(this.ConnectionName))
            {
                if (ConfigurationManager.ConnectionStrings[this.ConnectionName] == null ||
                    string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings[this.ConnectionName].ConnectionString))
                    throw new MongoConnectionException("The connection string name specified was not found.");

                return Provider().GetRepository(
                        ConfigurationManager.ConnectionStrings[this.ConnectionName].ConnectionString,
                        this.Database);
            }
            
            // We have a connection string
            if (!string.IsNullOrWhiteSpace(this.ConnectionString))
                return Provider().GetRepository(this.ConnectionString, this.Database);

            // No connection strings at all, use the old method using the properties
            var database = this.Database;
            var settings = new MongoServerSettings {Server = new MongoServerAddress(this.Host, this.Port)};
            
            if (HasCredentials)
                settings.DefaultCredentials = new MongoCredentials(this.Username, this.Password);

            return Provider().GetRepository(settings, database);
        }

        private bool HasCredentials { get { return !string.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password); }}

        #endregion

        #region Public Methods

        internal void TestWrite(LogEventInfo logEvent)
		{
			Write(logEvent);
		}

		protected override void Write(LogEventInfo logEvent)
		{
			using (var repository = GetRepository())
			{
				repository.Insert(logEvent);
			}
        }

        #endregion

    }
}
