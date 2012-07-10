using System;
using System.Configuration;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using NUnit.Framework;

namespace NLog.MongoDB.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void Test_ConnectionName()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            var server = MongoServer.Create(connectionString);

            TestMongoConnection(
                server, 
                connectionString.ParseDatabaseName(),
                "testMongoConnectionName");
        }

        [Test]
        public void Test_ConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            var server = MongoServer.Create(connectionString);

            TestMongoConnection(
                server,
                connectionString.ParseDatabaseName(),
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
            var collection = db.GetCollection<LogEventInfoData>(loggerName);

            // Clear out test collection
            collection.RemoveAll();

            var logger = LogManager.GetLogger(loggerName);

            logger.LogException(
                LogLevel.Error,
                "Test Log Message",
                new Exception("Test Exception"));

            collection.FindAll().Count()
                .Should().Be(1);

            var logEntry = collection.FindAll().First();

            logEntry.Level
                .Should().Be(LogLevel.Error.ToString());
            logEntry.Message
                .Should().Be("Test Log Message");
            logEntry.Exception.Message
                .Should().Be("Exception of type 'System.Exception' was thrown.");

            // Clean-up
            db.DropCollection(loggerName);
            server.Disconnect();
        }

        #endregion

    }
}