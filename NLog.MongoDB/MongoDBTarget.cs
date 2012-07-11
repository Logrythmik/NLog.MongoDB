using System;
using System.Collections.Generic;
using System.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Config;
using NLog.Targets;

namespace NLog.MongoDB
{
	[Target("MongoDB")]
	public sealed class MongoDBTarget : Target
	{
		public Func<IRepositoryProvider> Provider = () => new MongoServerProvider();

		public MongoDBTarget()
		{
			Fields = new List<MongoDBTargetField>();
		}

        #region Exposed Properties

		[ArrayParameter(typeof(MongoDBTargetField), "field")]
		public IList<MongoDBTargetField> Fields { get; private set; }

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

		private BsonDocument BuildBsonDocument(LogEventInfo logEvent)
		{
			if (Fields.Count == 0)
				return BuildFullBsonDocument(logEvent);

			var doc = new BsonDocument();
			foreach (var field in Fields)
			{
				var value = field.Layout.Render(logEvent);

				doc.Add(field.Name, BsonValue.Create(value));
			}

			return doc;
		}

		private BsonDocument BuildFullBsonDocument(LogEventInfo logEvent)
		{
			var doc = new BsonDocument();
			doc["sequenceID"] = logEvent.SequenceID;
			doc["timeStamp"] = logEvent.TimeStamp;
			doc["machineName"] = Environment.MachineName;

			if (logEvent.LoggerName != null)
				doc["loggerName"] = logEvent.LoggerName;
			if (logEvent.Message != null)
				doc["message"] = logEvent.Message;
			if (logEvent.FormattedMessage != null)
				doc["formattedMessage"] = logEvent.FormattedMessage;
			if (logEvent.Level != null)
				doc["level"] = logEvent.Level.ToString();
			if (logEvent.StackTrace != null)
				doc["stackTrace"] = logEvent.StackTrace.ToString();

			if (logEvent.UserStackFrame != null)
			{
				doc["userStackFrame"] = logEvent.UserStackFrame.ToString();
				doc["UserStackFrameNumber"] = logEvent.UserStackFrameNumber;
			}

			if (logEvent.Exception != null)
			{
				doc["exception"] = BuildExceptionBsonDocument(logEvent.Exception);
			}

			if (logEvent.Properties != null && logEvent.Properties.Count > 0)
			{
				doc["properties"] = BuildPropertiesBsonDocument(logEvent.Properties);
			}

			if (logEvent.Parameters != null && logEvent.Parameters.Length > 0)
			{
				doc["Parameters"] = logEvent.Parameters.ToBson(); ;
			}

			return doc;
		}

		private BsonDocument BuildPropertiesBsonDocument(IDictionary<object, object> properties)
		{
			var doc = new BsonDocument();
			foreach (var entry in properties)
			{
				doc[entry.Key.ToString()] = entry.Value.ToString();
			}
			return doc;
		}

		private BsonDocument BuildExceptionBsonDocument(Exception ex)
		{
			var doc = new BsonDocument();
			doc["message"] = ex.Message;
			doc["source"] = ex.Source ?? string.Empty;
			doc["stackTrace"] = ex.StackTrace ?? string.Empty;

			if (ex.InnerException != null)
			{
				doc["innerException"] = BuildExceptionBsonDocument(ex.InnerException);
			}

			return doc;
		}

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
				repository.Insert(
					logEvent.LoggerName,
					BuildBsonDocument(logEvent));
			}
        }

        #endregion
    }
}
