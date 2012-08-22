using System;
using MongoDB.Bson;

namespace NLog.MongoDB
{
	public interface IRepository : IDisposable
	{
		void Insert(string collectionName, BsonDocument item);
	    void CheckCollection(string collectionName, bool useCappedCollection, long? cappedCollectionSize, long? cappedCollectionMaxItems, bool createIdField);
	}
}