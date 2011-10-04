using System;

namespace NLog.MongoDB
{
	public interface IRepository : IDisposable
	{
		void Insert(LogEventInfo item);
	}
}