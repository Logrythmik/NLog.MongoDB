using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MongoDB.Driver;
using NUnit.Framework;

namespace NLog.MongoDB.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
		[Test]
		public void Test_DynamicFields()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
			var server = MongoServer.Create(connectionString);
		    var connectionStringBuilder = new MongoConnectionStringBuilder(connectionString);
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
            
			logEntry["level"].Should().Be(LogLevel.Error.ToString());
            logEntry["message"].Should().Be("Test Log Message");
            logEntry["exception"].Should().Be("Test Exception");
            
            // Clean-up
            db.DropCollection(loggerName);
            server.Disconnect();
		}

        [Test]
        public void Test_ConnectionName()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            var server = MongoServer.Create(connectionString);
            var connectionStringBuilder = new MongoConnectionStringBuilder(connectionString);

            TestMongoConnection(
                server,
                connectionStringBuilder.DatabaseName,
                "testMongoConnectionName");
        }

        [Test]
        public void Test_ConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            var server = MongoServer.Create(connectionString);
            var connectionStringBuilder = new MongoConnectionStringBuilder(connectionString);

			TestMongoConnection(
				server,
                connectionStringBuilder.DatabaseName,
				"testMongoConnectionString");
        }

        [Test]
        public void Test_OldWay()
        {
            var server = new MongoServer(
                new MongoServerSettings
                    {
                        Server = new MongoServerAddress("ds035607.mongolab.com", 35607),
                        DefaultCredentials = new MongoCredentials("mongo", "db")
                    });

            TestMongoConnection(
                server,
                "nlog",
                "testMongo"
                );
        }

        #region Helpers

        private void TestMongoConnection(
            MongoServer server, 
            string database,
            string loggerName)
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

            collection.FindAll().Count()
                .Should().Be(1);

            var logEntry = collection.FindAll().First();

            logEntry["level"]
                .Should().Be(LogLevel.Error.ToString());
            logEntry["message"]
                .Should().Be("Test Log Message");
            
			var exception = logEntry["exception"].AsBsonDocument;
			
			exception["message"]
                .Should().Be("Test Exception");
            
			var innerException = exception["innerException"].AsBsonDocument;
			
			innerException["message"]
                .Should().Be("Inner Exception");

            // Clean-up
            db.DropCollection(loggerName);
            server.Disconnect();
        }

        #endregion

    }
}