using System;
using System.Reflection;
using MongoDB.Bson;

namespace NLog.MongoDB
{
	public static class LogEventInfoExtensions
	{
		public static FieldSearchResult GetValue(this LogEventInfo logEvent, string propertyName)
		{
			var property = typeof(LogEventInfo).GetProperty(
				propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

			if (property != null)
				return new FieldSearchResult
				{
					Succeded = true,
					Value = property.GetValue(logEvent, BindingFlags.Public, null, null, null)
				};

			var isInProperties = logEvent.Properties.ContainsKey(propertyName);
			return new FieldSearchResult
			{
				Succeded = isInProperties,
				Value = isInProperties ? logEvent.Properties[propertyName] : null
			};
		}

		public static BsonDocument ToBsonDocument(this LogEventInfo logEvent)
		{
			var doc = new BsonDocument();

			doc.AddField("sequenceID", logEvent.SequenceID);
			doc.AddField("timeStamp", logEvent.TimeStamp);
			doc.AddField("machineName", Environment.MachineName);
			doc.AddField("loggerName", logEvent.LoggerName);
			doc.AddField("message", logEvent.Message);
			doc.AddField("formattedMessage", logEvent.FormattedMessage);
			doc.AddField("level", logEvent.Level);
			doc.AddField("stackTrace", logEvent.StackTrace);
			doc.AddField("userStackFrame", logEvent.UserStackFrame);
			doc.AddField("UserStackFrameNumber", logEvent.UserStackFrameNumber);
			doc.AddField("exception", logEvent.Exception);
			doc.AddField("properties", logEvent.Properties);
			doc.AddField("parameters", logEvent.Parameters);

			return doc;
		}
	}
}