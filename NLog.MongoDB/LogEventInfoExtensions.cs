using System.Reflection;

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
	}
}