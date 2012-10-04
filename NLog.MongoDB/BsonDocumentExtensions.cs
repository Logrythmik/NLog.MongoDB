using System;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;

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
                    BsonArray array = new BsonArray();
                    foreach (var param in parameters)
                    {
                        array.Add(BsonValue.Create(param));
                    }
                    doc[name] = array;
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

            if (ex.Data != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    string keyStr = key.ToString();
                    //used to make sure that the data does not conflict with properties of the exception
                    if (keyStr == "message" || keyStr == "source" || keyStr == "stackTrace" || keyStr == "innerException")
                        keyStr += "_data";

                    doc[keyStr] = BsonValue.Create(ex.Data[key]);
                }
            }

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