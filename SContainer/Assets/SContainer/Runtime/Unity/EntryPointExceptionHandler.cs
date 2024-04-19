using System;

namespace SContainer.Runtime.Unity
{
    internal sealed class EntryPointExceptionHandler
    {
        private readonly Action<Exception> handler;

        public EntryPointExceptionHandler(Action<Exception> handler)
        {
            this.handler = handler;
        }

        public void Publish(Exception exception)
        {
            this.handler.Invoke(exception);
        }
    }
}