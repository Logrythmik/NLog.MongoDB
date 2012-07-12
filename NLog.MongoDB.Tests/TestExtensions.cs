using System;
using FluentAssertions;
using NUnit.Framework;

namespace NLog.MongoDB.Tests
{
    [TestFixture]
    public class TestExtensions
    {
        [Test]
        public void TestTheHellOutOf_ParseDatabaseName()
        {
            "".Invoking(s => s.ParseDatabaseName())
                .ShouldThrow<FormatException>();

            ((string)null)
                .Invoking(s => s.ParseDatabaseName())
                .ShouldThrow<FormatException>();

            "some random string with // stuff in it"
                .Invoking(s => s.ParseDatabaseName())
                .ShouldThrow<FormatException>();

            var dbName = "foo";

            ("mongodb://some.server.name:231/" + dbName)
                .ParseDatabaseName()
                .Should().Be("foo");

            dbName = "a-really-long-MixedCaseDatabaseName";

            ("mongodb://user:password@some.server.name:231/" + dbName)
                .ParseDatabaseName()
                .Should().Be(dbName);

            dbName = "sumthin_funky-with-numbers_1234";

            ("mongodb://user:password@some.server.name:231/" + dbName + "?and=some options")
                .ParseDatabaseName()
                .Should().Be(dbName);

            ("mongodb://user:password@server.one:231,server.two:231/" + dbName + "?and=some options")
                .ParseDatabaseName()
                .Should().Be(dbName);

            ("mongodb://user:password@server.one,server.two:231/" + dbName + "?and=some options")
                .ParseDatabaseName()
                .Should().Be(dbName);

            ("mongodb://user:password@server.one,server.two:231,server.three,server.for:0987/" + dbName + "?and=some options")
                .ParseDatabaseName()
                .Should().Be(dbName);

            ("mongodb://user:password@server.one:231,server.two.with.alot.of.subdomains/" + dbName + "?and=some options")
                .ParseDatabaseName()
                .Should().Be(dbName);
            
            ("mongodb://localhost/" + dbName)
                .ParseDatabaseName()
                .Should().Be(dbName);

            ("mongodb://fred:foobar@localhost/" + dbName)
                .ParseDatabaseName()
                .Should().Be(dbName);

            "mongodb://fred:foobar@localhost/baz"
                .ParseDatabaseName()
                .Should().Be("baz");

            ("mongodb://example1.com:27017,example2.com:27017/" + dbName)
                .ParseDatabaseName()
                .Should().Be(dbName);

            "mongodb://localhost,localhost:27018,localhost:27019/"
                .ParseDatabaseName()
                .Should().BeNull();

            "mongodb://host1,host2,host3/?slaveOk=true"
                .ParseDatabaseName()
                .Should().BeNull();

        }
        
    }
}