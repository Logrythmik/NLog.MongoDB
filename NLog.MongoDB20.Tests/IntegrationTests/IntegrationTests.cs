using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NLog.MongoDB20.Tests
{
	[TestClass]
	public class IntegrationTests
	{
		private IMongoDatabase _db;
		private MongoClient _client;

		[TestInitialize]
		public void Init()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var connectionStringBuilder = new MongoUrlBuilder(connectionString);

			_client = new MongoClient(connectionStringBuilder.ToMongoUrl());

			var dbName = connectionStringBuilder.DatabaseName;

			_db = _client.GetDatabase(dbName);
		}

		[TestCleanup]
		public void CleanUp()
		{

		}

		[TestMethod]
		public void Test_DynamicFields()
		{
			const string loggerName = "testDynamicFields";

			var collection = _db.GetCollection<BsonDocument>(loggerName);

            // Clear out test collection
            var filter = new BsonDocument();
            collection.DeleteManyAsync(filter);

			var logger = LogManager.GetLogger(loggerName);

			logger.Log(
				LogLevel.Error,
				"Test Log Message",
				new Exception("Test Exception", new Exception("Inner Exception")));

			Thread.Sleep(2000);

            collection.Find(filter).CountAsync().Result.Should().Be(1);

			var logEntry = collection.Find(filter).FirstAsync().Result;

			Assert.IsTrue(logEntry.Contains("_id"));

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");
            logEntry["exception"].Should();

			var logException = logEntry["exception"];
            logException["message"].Should().Be("Test Exception");

            // Clean-up
            _db.DropCollectionAsync(loggerName);
		}
		[TestMethod]
		public void Test_DynamicFields_Without_Exception()
		{

			const string loggerName = "testDynamicFields";

			var collection = _db.GetCollection<BsonDocument>(loggerName);

            // Clear out test collection
            var filter = new BsonDocument();
            collection.DeleteManyAsync(filter);

            var logger = LogManager.GetLogger(loggerName);

			logger.Log(
				LogLevel.Error,
				"Test Log Message");

			Thread.Sleep(2000);

            collection.Find(filter).CountAsync().Result.Should().Be(1);

            var logEntry = collection.Find(filter).FirstAsync().Result;

            Assert.IsTrue(logEntry.Contains("_id"));
			Assert.IsFalse(logEntry.Contains("exception"));

			logEntry["level"].Should().Be(LogLevel.Error.ToString());
			logEntry["message"].Should().Be("Test Log Message");

            // Clean-up
            _db.DropCollectionAsync(loggerName);
        }

        [TestMethod]
        public void Test_DynamicTypedFields()
        {
            const string loggerName = "testDynamicTypedFields";

            var collection = _db.GetCollection<BsonDocument>(loggerName);

            // Clear out test collection
            var filter = new BsonDocument();
            collection.DeleteManyAsync(filter);

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
            collection.Find(filter).CountAsync().Result.Should().Be(1);

            var logEntry = collection.Find(filter).FirstAsync().Result;

            Assert.IsTrue(logEntry.Contains("_id"));

            Assert.AreEqual(logEventTime.Date, logEntry["timeStamp"].ToUniversalTime().Date);

            logEntry["level"].Should().Be(LogLevel.Error.ToString());
            logEntry["message"].Should().Be("Test Log Message");

            var exception = logEntry["exception"].AsBsonDocument;
            Assert.AreEqual("Test Exception", exception["message"].AsString);

            var innerException = exception["innerException"].AsBsonDocument;
            Assert.AreEqual("Inner Exception", innerException["message"].AsString);
            var properties = logEntry["properties"];
            Assert.AreEqual("1", properties["transactionId"]);

            // Clean-up
            _db.DropCollectionAsync(loggerName);
        }

        [TestMethod]
        public void Test_Capped_Collection_With_Id()
        {
            const string loggerName = "cappedWithId";

            _db.DropCollectionAsync(loggerName);

            var collection = _db.GetCollection<BsonDocument>(loggerName);

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

            collection.Find(new BsonDocument()).CountAsync().Result.Should().Be(1);

            var logEntry = collection.Find(new BsonDocument()).FirstAsync().Result;

            Assert.IsTrue(logEntry.Contains("_id"));

            // Clean-up
            _db.DropCollectionAsync(loggerName);
        }



        [TestMethod]
        public void Test_ConnectionName()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            var connectionStringBuilder = new MongoUrlBuilder(connectionString);

            TestMongoConnection(
                _client,
                connectionStringBuilder.DatabaseName,
                "testMongoConnectionName");
        }

        [TestMethod]
        public void Test_ConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            var connectionStringBuilder = new MongoUrlBuilder(connectionString);

            TestMongoConnection(
                _client,
                connectionStringBuilder.DatabaseName,
                "testMongoConnectionString");
        }

        #region Helpers

        private void TestMongoConnection(MongoClient client, string database, string loggerName)
        {
            var db = client.GetDatabase(database);
            var collection = db.GetCollection<BsonDocument>(loggerName);

            // Clear out test collection

            var filter = new BsonDocument();
            collection.DeleteManyAsync(filter);

            var logger = LogManager.GetLogger(loggerName);

            logger.Log(
                LogLevel.Error,
                "Test Log Message",
                new Exception("Test Exception", new Exception("Inner Exception")));

            Thread.Sleep(2000);
            collection.Find(new BsonDocument()).CountAsync().Result.Should().Be(1);

            var logEntry = collection.Find(new BsonDocument()).FirstAsync().Result;


            Assert.IsTrue(logEntry.Contains("_id"));

            logEntry["level"].Should().Be(LogLevel.Error.ToString());
            logEntry["message"].Should().Be("Test Log Message");

            var exception = logEntry["exception"].AsBsonDocument;

            exception["message"].Should().Be("Test Exception");

            var innerException = exception["innerException"].AsBsonDocument;

            innerException["message"].Should().Be("Inner Exception");

            // Clean-up
            _db.DropCollectionAsync(loggerName);

        }

        #endregion
    }
}