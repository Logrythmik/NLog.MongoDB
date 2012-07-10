using System;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace NLog.MongoDB.Tests
{
	[TestFixture]
	public class TestMongoTarget
	{
		private Mock<IRepositoryProvider> _mockProvider;
		private Mock<IRepository> _mockRepository;
		private MongoServerSettings _settings;

		[SetUp]
		public void TestTarget()
		{
			_mockProvider = new Mock<IRepositoryProvider>();
			_mockRepository = new Mock<IRepository>();

			_settings = new MongoServerSettings();
		}

		[Test]
		public void TestSettingsAndRepository()
		{
			var connectionName = "Logs";

			_mockProvider.Setup(
				p => p.GetRepository(It.IsAny<string>()))
				.Returns(_mockRepository.Object)
				.Verifiable();

			var target = new MongoDBTarget(_mockProvider.Object)
			{
			    ConnectionName = connectionName
			};

			var eventLogInfo = new LogEventInfo();

			_mockRepository.Setup(
				r => r.Insert(eventLogInfo))
				.Verifiable();
			
			target.TestWrite(eventLogInfo);

			_mockProvider.Verify();
			_mockRepository.Verify();
		}

		[Test]
        [Ignore("Mongo C# driver 1.4.2 doesn't seem to serialize exceptions correctly")]
		public void TestActualLog()
		{
			var logger = LogManager.GetLogger("MyTestClass");
			var server = new MongoServer(new MongoServerSettings
				{
					Server = new MongoServerAddress("localhost", 27017)
				});
			var db = server.GetDatabase("NLog");
			var collection = db.GetCollection<LogEventInfoData>("MyTestClass");

			collection.RemoveAll();

			logger.LogException(
				LogLevel.Error, "Test Log Message",
				new Exception("Test Exception"));
				
			collection.FindAll().Count()
                .Should().Be(1);

			var logEntry = collection.FindAll().First();

			logEntry.Level
                .Should().Be(LogLevel.Error.ToString());
			logEntry.Message
                .Should().Be("Test Log Message");
			logEntry.Exception.Message
                .Should().Be("Test Exception");
				
			
		}
	}
}
