namespace NLog.MongoDB
{
	public sealed class ExceptionInfo
	{
		public string Message { get; set; }
		public string Source { get; set; }
		public string StackTrace { get; set; }
		public ExceptionInfo InnerException { get; set; }
	}
}
