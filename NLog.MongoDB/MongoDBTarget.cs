using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using NLog;
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

		public string Database
		{
			get { return _Database ?? "NLog"; }
			set { _Database = value; }
		}
		private string _Database;

		internal void TestWrite(LogEventInfo logEvent)
		{
			Write(logEvent);
		}

		protected override void Write(LogEventInfo logEvent)
		{

			using (var repository = Provider().GetRepository(
							new MongoServerSettings {
			            		Server = new MongoServerAddress(this.Host, this.Port)
			            	}, 
							this.Database))
			{
				repository.Insert(logEvent);
			}
		}
	}
}
