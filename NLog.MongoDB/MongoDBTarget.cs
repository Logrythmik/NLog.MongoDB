using System;
using MongoDB.Driver;
using NLog.Targets;

namespace NLog.MongoDB
{
	[Target("MongoDB")]
	public sealed class MongoDBTarget : Target
	{
		public Func<IRepositoryProvider> Provider = () => new MongoServerProvider();
		
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

       		private bool HasCredentials { get { return !string.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password); }}

		internal void TestWrite(LogEventInfo logEvent)
		{
			Write(logEvent);
		}

		protected override void Write(LogEventInfo logEvent)
		{
		    var settings = new MongoServerSettings { Server = new MongoServerAddress(this.Host, this.Port) };
            
            if (HasCredentials)
                settings.DefaultCredentials = new MongoCredentials(this.Username, this.Password);

			using (var repository = Provider().GetRepository(settings, this.Database))
			{
				repository.Insert(logEvent);
			}
		}
	}
}
