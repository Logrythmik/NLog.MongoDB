using System;
using MongoDB.Bson;

namespace NLog.MongoDB20
{
	public interface IRepository : IDisposable
	{
		void Insert(string collectionName, BsonDocument item);
	    
		void CheckCollection(string collectionName, long collectionSize, long? collectionMaxItems, bool createIdField);
	}
}