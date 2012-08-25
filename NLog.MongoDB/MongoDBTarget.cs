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
		public Func<IRepositoryProvider> GetProvider = () => new MongoServerProvider();

		public MongoDBTarget()
		{
			Fields = new List<MongoDBTargetField>();
		}

        #region Exposed Properties

		[ArrayParameter(typeof(MongoDBTargetField), "field")]
		public IList<MongoDBTargetField> Fields { get; private set; }

        public string ConnectionString { get; set; }

        public string ConnectionName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

	    public string CollectionName { get; set; }

	    public bool UseCappedCollection { get; set; }

	    public bool CreateIdField { get; set; }

        public bool AppendFields { get; set; }

        public long? CappedCollectionSize { get; set; }

        public long? CappedCollectionMaxItems { get; set; }

        #region Defaulted Properties

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

        #endregion

        #endregion

        #region Private Helpers

        internal IRepository GetRepository()
        {
            // We have a connection string name, grab this from the config.
            if (!string.IsNullOrWhiteSpace(this.ConnectionName))
            {
                if (ConfigurationManager.ConnectionStrings[this.ConnectionName] == null ||
                    string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings[this.ConnectionName].ConnectionString))
                    throw new MongoConnectionException("The connection string name specified was not found.");

                this.ConnectionString = ConfigurationManager.ConnectionStrings[this.ConnectionName].ConnectionString;
            }

            MongoUrlBuilder mongoUrlBuilder;
            // We have a connection string
            if (!string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                mongoUrlBuilder = new MongoUrlBuilder(this.ConnectionString);

                if (!string.IsNullOrEmpty(this.Database))
                {
                    mongoUrlBuilder.DatabaseName = this.Database;
                }
                else if (!String.IsNullOrEmpty(mongoUrlBuilder.DatabaseName))
                {
                    this.Database = mongoUrlBuilder.DatabaseName;
                }
            }
            // No connection strings at all, use the old method using the properties                    
            else
            {
                mongoUrlBuilder = new MongoUrlBuilder();
                mongoUrlBuilder.DatabaseName = this.Database;
                mongoUrlBuilder.Server = new MongoServerAddress(this.Host, this.Port);

                if (HasCredentials) mongoUrlBuilder.DefaultCredentials = new MongoCredentials(this.Username, this.Password);
            }

            return GetProvider().GetRepository(mongoUrlBuilder.ToServerSettings(), mongoUrlBuilder.DatabaseName);
        }

	    private bool HasCredentials
	    {
	        get
	        {
	            return !string.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password);
	        }
	    }

        internal BsonDocument BuildBsonDocument(LogEventInfo logEvent)
        {
            BsonDocument doc;

			if (Fields.Count == 0 || this.AppendFields)
			{
			    doc = BuildFullBsonDocument(logEvent);
			}
			else
			{
			    doc = new BsonDocument();
                if (this.CreateIdField) doc["_id"] = ObjectId.GenerateNewId();
			}

			foreach (var field in Fields)
			{
				var value = field.Layout.Render(logEvent);
				doc[field.Name] = BsonValue.Create(value);
			}

			return doc;
		}

        internal BsonDocument BuildFullBsonDocument(LogEventInfo logEvent)
		{
			var doc = new BsonDocument();
            if (this.CreateIdField) doc["_id"] = ObjectId.GenerateNewId();

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
				doc["userStackFrameNumber"] = logEvent.UserStackFrameNumber;
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
				doc["parameters"] = logEvent.Parameters.ToJson();
			}

			return doc;
		}

        internal BsonDocument BuildPropertiesBsonDocument(IDictionary<object, object> properties)
		{
			var doc = new BsonDocument();
			foreach (var entry in properties)
			{
				doc[entry.Key.ToString()] = entry.Value.ToString();
			}
			return doc;
		}

        internal BsonDocument BuildExceptionBsonDocument(Exception ex)
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

#if DEBUG

        internal void TestWrite(LogEventInfo logEvent)
		{
			Write(logEvent);
		}

#endif

		protected override void Write(LogEventInfo logEvent)
		{
			using (var repository = GetRepository())
			{
			    string collectionName = logEvent.LoggerName;
                if (!string.IsNullOrEmpty(this.CollectionName)) collectionName = this.CollectionName;

			    repository.CheckCollection(collectionName, this.UseCappedCollection, this.CappedCollectionSize, this.CappedCollectionMaxItems, this.CreateIdField);
                repository.Insert(collectionName, BuildBsonDocument(logEvent));
			}
        }

    }
}
