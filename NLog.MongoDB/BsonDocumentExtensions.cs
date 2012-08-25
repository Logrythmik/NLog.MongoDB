using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace NLog.MongoDB
{
	public static class BsonDocumentExtensions
	{
		public static void AddField(this BsonDocument doc, string name, object value)
		{
			if (value == null) return;

			var unifiedName = name.ToLower();
			switch (unifiedName)
			{
				case "exception":
					doc[name] = BuildExceptionBsonDocument((Exception)value); break;

				case "properties":
					var properties = (IDictionary<object, object>)value;
					if (properties.Count > 0)
						doc[name] = BuildPropertiesBsonDocument(properties);
					break;

				case "parameters":
					var parameters = (object[])value;
					if (parameters.Length > 0)
						doc[name] = parameters.ToBson();
					break;

				default:
					BsonValue bsonValue;
					if (BsonTypeMapper.TryMapToBsonValue(value, out bsonValue))
						doc[name] = bsonValue;
					else
						doc[name] = BsonValue.Create(value.ToString());
					break;
			}
		}

		private static BsonDocument BuildExceptionBsonDocument(Exception ex)
		{
			var doc = new BsonDocument();
			doc["message"] = ex.Message;
			doc["source"] = ex.Source ?? string.Empty;
			doc["stackTrace"] = ex.StackTrace ?? string.Empty;

			if (ex.InnerException != null)
			{
				doc["innerException"] = BuildExceptionBsonDocument(ex.InnerException);
			}

			return doc;
		}

		private static BsonDocument BuildPropertiesBsonDocument(IDictionary<object, object> properties)
		{
			var doc = new BsonDocument();
			foreach (var entry in properties)
			{
				doc[entry.Key.ToString()] = entry.Value.ToString();
			}
			return doc;
		}
	}
}