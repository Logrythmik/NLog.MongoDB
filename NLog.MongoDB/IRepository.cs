namespace NLog.MongoDB
{
	public interface IRepository
	{
		void Insert(LogEventInfo item);
	}
}