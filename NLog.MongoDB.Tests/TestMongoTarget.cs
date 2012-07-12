using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using Moq;
using NLog.Common;
using NUnit.Framework;
using FluentAssertions;
using MongoDB.Bson;

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
            var databaseName = "Test";
            var host = "localhost";
            var port = 27017;

            _mockProvider.Setup(
                p => p.GetRepository(It.IsAny<MongoServerSettings>(), It.IsAny<string>()))
                .Returns(_mockRepository.Object)
                .Verifiable();

            var target = new MongoDBTarget
                             {
                                 Database = databaseName,
                                 Host = host,
                                 Port = port,
                                 Provider = () => _mockProvider.Object
                             };

            var eventLogInfo = new LogEventInfo();

            _mockRepository.Setup(
                r => r.Insert(It.IsAny<string>(), It.IsAny<BsonDocument>()))
                .Verifiable();

            target.TestWrite(eventLogInfo);

            _mockProvider.Verify();
            _mockRepository.Verify();

            new MongoDBTarget().Host
                .Should().Be(host);
            new MongoDBTarget().Port
                .Should().Be(port);
            new MongoDBTarget().Database
                .Should().Be("NLog");
        }

    }
}
