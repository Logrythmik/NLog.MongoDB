using NLog.Config;
using NLog.Layouts;

namespace NLog.MongoDB
{
	[NLogConfigurationItem]
	public sealed class MongoDBTargetField
	{
		public MongoDBTargetField() { }

		public MongoDBTargetField(string name, Layout layout)
		{
			Name = name;
			Layout = layout;
		}

		[RequiredParameter]
		public string Name { get; set; }

		public Layout Layout { get; set; }
	}
}
