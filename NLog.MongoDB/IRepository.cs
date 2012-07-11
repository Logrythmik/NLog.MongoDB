using System;
using MongoDB.Bson;

namespace NLog.MongoDB
{
	public interface IRepository : IDisposable
	{
		void Insert(string collectionName, BsonDocument item);
	}
}