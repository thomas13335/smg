using System;

namespace SMG.Common.Exceptions
{
    public class SyntaxErrorEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public SyntaxErrorEventArgs(Exception ex)
        {
            Error = ex;
        }
    }

    public delegate void SyntaxErrorHandler(object sender, SyntaxErrorEventArgs e);
}
