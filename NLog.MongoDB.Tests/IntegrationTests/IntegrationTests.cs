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
		[TestMethod]
		public void Test_DynamicFields()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);
			var dbName = connectionStringBuilder.DatabaseName;
			var loggerName = "testDynamicFields";

			var db = server.GetDatabase(dbName);
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
			logEntry["exception"].Should().Be("Test Exception");

			// Clean-up
			db.DropCollection(loggerName);
			server.Disconnect();
		}

		[TestMethod]
		public void Test_DynamicTypedFields()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);
			var dbName = connectionStringBuilder.DatabaseName;
			var loggerName = "testDynamicTypedFields";

			var db = server.GetDatabase(dbName);
			var collection = db.GetCollection(loggerName);
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

			Assert.AreEqual(logEventTime.Date, logEntry["timestamp"].AsDateTime.Date);

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");

			var exception = logEntry["exception"].AsBsonDocument;
			Assert.AreEqual("Test Exception", exception["message"].AsString);

			var innerException = exception["innerException"].AsBsonDocument;
			Assert.AreEqual("Inner Exception", innerException["message"].AsString);

			Assert.AreEqual(1, logEntry["transactionId"].AsInt32);

			db.DropCollection(loggerName);
			server.Disconnect();
		}

		[TestMethod]
		public void Test_Capped_Collection_With_Id()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);
			var dbName = connectionStringBuilder.DatabaseName;
			var loggerName = "cappedWithId";

			var db = server.GetDatabase(dbName);
			db.DropCollection(loggerName);
			var collection = db.GetCollection(loggerName);

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

			db.DropCollection(loggerName);
			server.Disconnect();
		}

		[TestMethod]
		public void Test_Capped_Collection_Without_Id()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);
			var dbName = connectionStringBuilder.DatabaseName;
			var loggerName = "cappedWithoutId";

			var db = server.GetDatabase(dbName);
			db.DropCollection(loggerName);
			var collection = db.GetCollection(loggerName);

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

			Assert.IsTrue(collection.IsCapped());
			Assert.IsFalse(logEntry.Contains("_id"));
			Assert.IsFalse(collection.Settings.AssignIdOnInsert);

			db.DropCollection(loggerName);
			server.Disconnect();
		}
		
		[TestMethod]
		public void Test_ConnectionName()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);

			TestMongoConnection(
				server,
				connectionStringBuilder.DatabaseName,
				"testMongoConnectionName");
		}

		[TestMethod]
		public void Test_ConnectionString()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);

			TestMongoConnection(
				server,
				connectionStringBuilder.DatabaseName,
				"testMongoConnectionString");
		}

		[TestMethod]
		public void Test_OldWay()
		{
			var settings = new MongoServerSettings
			{
				Server = new MongoServerAddress("ds035607.mongolab.com", 35607),
				DefaultCredentials = new MongoCredentials("mongo", "db")
			};

			TestMongoConnection(new MongoServer(settings), "nlog", "testMongo");
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