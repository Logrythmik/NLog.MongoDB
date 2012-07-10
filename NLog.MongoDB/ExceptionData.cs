using System;

namespace NLog.MongoDB
{
    public sealed class ExceptionData
    {
        public ExceptionData(Exception exc)
        {
            this.Message = exc.Message;
            this.Source = exc.Source;
            this.StackTrace = exc.StackTrace;
            
            if(exc.InnerException!=null)
                this.InnerException = new ExceptionData(exc.InnerException);
        }

        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public ExceptionData InnerException { get; set; }
    }
}