using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MongoDB.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.MongoDB.Tests
{
	[TestClass]
	public class IntegrationTests
	{
		private MongoDatabase _db;
		private MongoServer _server;

		[TestInitialize]
		public void Init()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);

			var mongoClient = new MongoClient(connectionStringBuilder.ToMongoUrl());

			_server = mongoClient.GetServer();
			
			var dbName = connectionStringBuilder.DatabaseName;

			_db = _server.GetDatabase(dbName);
		}

		[TestCleanup]
		public void CleanUp()
		{
			_server.Disconnect();
		}

		[TestMethod]
		public void Test_DynamicFields()
		{
			const string loggerName = "testDynamicFields";

			var collection = _db.GetCollection(loggerName);

			// Clear out test collection
			collection.RemoveAll();

			var logger = LogManager.GetLogger(loggerName);

			logger.LogException(
				LogLevel.Error,
				"Test Log Message",
				new Exception("Test Exception", new Exception("Inner Exception")));

			Thread.Sleep(2000);

			collection.FindAll().Count().Should().Be(1);

			var logEntry = collection.FindAll().First();

			Assert.IsTrue(logEntry.Contains("_id"));

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");
			logEntry["exception"].Should().Be("Test Exception");

			// Clean-up
			_db.DropCollection(loggerName);
		}
		[TestMethod]
		public void Test_DynamicFields_Without_Exception()
		{

			const string loggerName = "testDynamicFields";

			var collection = _db.GetCollection(loggerName);

			// Clear out test collection
			collection.RemoveAll();

			var logger = LogManager.GetLogger(loggerName);

			logger.Log(
				LogLevel.Error,
				"Test Log Message");

			Thread.Sleep(2000);

			collection.FindAll().Count().Should().Be(1);

			var logEntry = collection.FindAll().First();

			Assert.IsTrue(logEntry.Contains("_id"));
			Assert.IsFalse(logEntry.Contains("exception"));

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");

			// Clean-up
			_db.DropCollection(loggerName);
		}

		[TestMethod]
		public void Test_DynamicTypedFields()
		{
			const string loggerName = "testDynamicTypedFields";

			var collection = _db.GetCollection(loggerName);
			collection.RemoveAll();

			var logger = LogManager.GetLogger(loggerName);
			var logEventTime = DateTime.UtcNow;

			var logEvent = new LogEventInfo
			{
				TimeStamp = logEventTime,
				LoggerName = loggerName,
				Level = LogLevel.Error,
				Message = "Test Log Message",
				Exception = new Exception("Test Exception", new Exception("Inner Exception"))
			};
			logEvent.Properties.Add("transactionId", 1);

			logger.Log(logEvent);
			Thread.Sleep(2000);

			collection.FindAll().Count().Should().Be(1);

			var logEntry = collection.FindAll().First();

			Assert.IsTrue(logEntry.Contains("_id"));

			Assert.AreEqual(logEventTime.Date, logEntry["timestamp"].ToUniversalTime().Date);

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");

			var exception = logEntry["exception"].AsBsonDocument;
			Assert.AreEqual("Test Exception", exception["message"].AsString);

			var innerException = exception["innerException"].AsBsonDocument;
			Assert.AreEqual("Inner Exception", innerException["message"].AsString);

			Assert.AreEqual(1, logEntry["transactionId"].AsInt32);

			_db.DropCollection(loggerName);
		}

		[TestMethod]
		public void Test_Capped_Collection_With_Id()
		{
			const string loggerName = "cappedWithId";

			_db.DropCollection(loggerName);

			var collection = _db.GetCollection(loggerName);

			var logger = LogManager.GetLogger(loggerName);
			var logEventTime = DateTime.UtcNow;

			var logEvent = new LogEventInfo
			{
				Level = LogLevel.Info,
				TimeStamp = logEventTime,
				LoggerName = loggerName
			};

			logger.Log(logEvent);

			Thread.Sleep(2000);

			collection.FindAll().Count().Should().Be(1);

			var logEntry = collection.FindAll().First();

			Assert.IsTrue(logEntry.Contains("_id"));

			_db.DropCollection(loggerName);
		}

		[TestMethod, Ignore]// "Mongo driver is adding an ID regardless of the settings we set."
		public void Test_Capped_Collection_Without_Id()
		{
			const string loggerName = "cappedWithoutId";

			_db.DropCollection(loggerName);
			
			var logger = LogManager.GetLogger(loggerName);
			var logEventTime = DateTime.UtcNow;

			var logEvent = new LogEventInfo
			{
				Level = LogLevel.Info,
				TimeStamp = logEventTime,
				LoggerName = loggerName
			};

			logger.Log(logEvent);
			Thread.Sleep(2000);

			var collection = _db.GetCollection(loggerName, new MongoCollectionSettings { AssignIdOnInsert = false });

			collection.FindAll().Count().Should().Be(1);

			var logEntry = collection.FindAll().First();

			collection.IsCapped()
				.Should().BeTrue("since we set it to be true in the configuration");

			logEntry.Contains("_id")
				.Should().BeFalse("since we set id-capture to false in the configuration");

			collection.Settings.AssignIdOnInsert
				.Should().BeFalse("since we set up the collection this way");

			_db.DropCollection(loggerName);
		}
		
		[TestMethod]
		public void Test_ConnectionName()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

			var connectionStringBuilder = new MongoUrlBuilder(connectionString);

			TestMongoConnection(
				_server,
				connectionStringBuilder.DatabaseName,
				"testMongoConnectionName");
		}

		[TestMethod]
		public void Test_ConnectionString()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

			var connectionStringBuilder = new MongoUrlBuilder(connectionString);

			TestMongoConnection(
				_server,
				connectionStringBuilder.DatabaseName,
				"testMongoConnectionString");
		}

		#region Helpers

		private void TestMongoConnection(MongoServer server, string database, string loggerName)
		{
			var db = server.GetDatabase(database);
			var collection = db.GetCollection(loggerName);

			// Clear out test collection
			collection.RemoveAll();

			var logger = LogManager.GetLogger(loggerName);

			logger.LogException(
				LogLevel.Error,
				"Test Log Message",
				new Exception("Test Exception", new Exception("Inner Exception")));

			Thread.Sleep(2000);

			collection.FindAll().Count().Should().Be(1);

			var logEntry = collection.FindAll().First();

			Assert.IsTrue(logEntry.Contains("_id"));

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");

			var exception = logEntry["exception"].AsBsonDocument;

			exception["message"].Should().Be("Test Exception");

			var innerException = exception["innerException"].AsBsonDocument;

			innerException["message"].Should().Be("Inner Exception");

			// Clean-up
			db.DropCollection(loggerName);
			server.Disconnect();
		}

		#endregion
	}
}