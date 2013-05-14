using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Layouts;

namespace NLog.MongoDB.Tests
{
    [TestClass]
    public class TestMongoTargetDocument
    {
        [TestMethod]
        public void TestConstructor()
        {
            const string name = "SomeName!";

            var doc = new MongoDBTargetDocument();
            doc.Name = name;

            doc.Name
                .Should().Be(name);
            doc.Fields.Count
                .Should().Be(0);
            doc.Documents.Count
                .Should().Be(0);
        }
    }
}
