using System;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using FluentAssertions;
using MongoDB.Bson;

namespace NLog.MongoDB.Tests
{
	[TestFixture]
	public class TestMongoTarget
	{
		private MockRepository _mock;
		private Mock<IRepositoryProvider> _mockProvider;
		private Mock<IRepository> _mockRepository;

		[SetUp]
		public void TestTarget()
		{
			_mock = new MockRepository(MockBehavior.Strict);

			_mockProvider = _mock.Create<IRepositoryProvider>();

			_mockRepository = _mock.Create<IRepository>();
			_mockRepository.Setup(r => r.Dispose());
		}

		[Test]
		public void TestDefaultSettings()
		{
			new MongoDBTarget().Host.Should().Be("localhost");
			new MongoDBTarget().Port.Should().Be(27017);
			new MongoDBTarget().Database.Should().Be("NLog");
		}

		[Test]
		public void TestSettings()
		{
			const string databaseName = "Test";
			const string host = "localhost";
			const int port = 27017;
			const string username = "someUser";
			const string password = "q198743n3d8yh32028##@!";
			const string connectionString = "mongodb://some.server/nlog";
			const string connectionName = "mongodb";
			const bool appendFields = false;
			const bool useCappedCollection = false;
			const int cappedCollectionMaxItems = 10000;
			const int cappedCollectionSize = 1000000;
			const bool createIdField = true;
			const string collectionName = "loggerName";

			var target = new MongoDBTarget
			{
				Database = databaseName,
				Host = host,
				Port = port,
				Username = username,
				Password = password,
				ConnectionString = connectionString,
				ConnectionName = connectionName,
				AppendFields = appendFields,
				UseCappedCollection = useCappedCollection,
				CappedCollectionMaxItems = cappedCollectionMaxItems,
				CappedCollectionSize = cappedCollectionSize,
				CreateIdField = createIdField,
				CollectionName = collectionName
			};

			target.Database.Should().Be(databaseName);
			target.Host.Should().Be(host);
			target.Port.Should().Be(port);
			target.Username.Should().Be(username);
			target.Password.Should().Be(password);
			target.ConnectionString.Should().Be(connectionString);
			target.ConnectionName.Should().Be(connectionName);
			target.AppendFields.Should().Be(appendFields);
			target.UseCappedCollection.Should().Be(useCappedCollection);
			target.CappedCollectionMaxItems.Should().Be(cappedCollectionMaxItems);
			target.CappedCollectionSize.Should().Be(cappedCollectionSize);
			target.CreateIdField.Should().Be(createIdField);
			target.CollectionName.Should().Be(collectionName);
		}

		[Test]
		public void TestRepository()
		{
			const string databaseName = "Test";
			const string host = "localhost";
			const int port = 27017;
			const string username = "someUser";
			const string password = "q198743n3d8yh32028##@!";

			_mockProvider
				.Setup(p => p.GetRepository(It.IsAny<MongoServerSettings>(), It.IsAny<string>()))
				.Returns(_mockRepository.Object);

			var target = new MongoDBTarget
			{
				Database = databaseName,
				Host = host,
				Port = port,
				Username = username,
				Password = password,
				GetProvider = () => _mockProvider.Object
			};

			var eventLogInfo = new LogEventInfo();

			_mockRepository
				.Setup(r => r.Insert(It.IsAny<string>(), It.IsAny<BsonDocument>()));

			target.TestWrite(eventLogInfo);

			_mock.VerifyAll();

			new MongoDBTarget().Host.Should().Be(host);
			new MongoDBTarget().Port.Should().Be(port);
			new MongoDBTarget().Database.Should().Be("NLog");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Should_Fail_If_Try_Use_Capped_Without_Size()
		{
			var target = new MongoDBTarget
			{
				UseCappedCollection = true
			};

			target.TestWrite(new LogEventInfo());
		}

		[Test]
		public void Should_Use_LoggerName_As_CollectionName()
		{
			_mockProvider
				.Setup(p => p.GetRepository(It.IsAny<MongoServerSettings>(), It.IsAny<string>()))
				.Returns(_mockRepository.Object);

			var target = new MongoDBTarget
			{
				GetProvider = () => _mockProvider.Object
			};

			var eventLogInfo = new LogEventInfo() { LoggerName = "loggerName" };

			_mockRepository.Setup(r => r.Insert("loggerName", It.IsAny<BsonDocument>()));

			target.TestWrite(eventLogInfo);

			_mock.VerifyAll();
		}

		[Test]
		public void Should_Use_CollectionName_From_Priority()
		{
			const string collectionName = "collectionName";

			_mockProvider
				.Setup(p => p.GetRepository(It.IsAny<MongoServerSettings>(), It.IsAny<string>()))
				.Returns(_mockRepository.Object);

			var target = new MongoDBTarget
			{
				CollectionName = collectionName,
				GetProvider = () => _mockProvider.Object
			};

			var eventLogInfo = new LogEventInfo() { LoggerName = "loggerName" };

			_mockRepository.Setup(r => r.Insert(collectionName, It.IsAny<BsonDocument>()));

			target.TestWrite(eventLogInfo);

			_mock.VerifyAll();
		}

		[Test]
		public void Shoul_Check_Collection_If_Use_Capped()
		{
			_mockProvider
				.Setup(p => p.GetRepository(It.IsAny<MongoServerSettings>(), It.IsAny<string>()))
				.Returns(_mockRepository.Object);

			var target = new MongoDBTarget
			{
				UseCappedCollection = true,
				CappedCollectionSize = 1,
				GetProvider = () => _mockProvider.Object
			};

			var eventLogInfo = new LogEventInfo() { LoggerName = "loggerName" };

			_mockRepository
				.Setup(r => r.Insert(It.IsAny<string>(), It.IsAny<BsonDocument>()));

			_mockRepository
				.Setup(r => r.CheckCollection(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<bool>()));

			target.TestWrite(eventLogInfo);

			_mock.VerifyAll();
		}
	}
}
