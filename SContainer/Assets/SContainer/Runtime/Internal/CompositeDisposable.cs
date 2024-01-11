using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    internal sealed class CompositeDisposable : IDisposable
    {
        private readonly Stack<IDisposable> disposables = new Stack<IDisposable>();
        
        public void Dispose()
        {
            IDisposable disposable;
            do
            {
                lock (this.disposables)
                {
                    disposable = this.disposables.Count > 0
                        ? this.disposables.Pop()
                        : null;
                }
                disposable?.Dispose();
            } while (disposable != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IDisposable disposable)
        {
            lock (this.disposables)
            {
                this.disposables.Push(disposable);
            }
        }
    }
}