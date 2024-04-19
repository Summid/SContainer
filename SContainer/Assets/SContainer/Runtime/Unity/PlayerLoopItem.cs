using SContainer.Runtime.Annotations;
using System;
using System.Collections.Generic;

namespace SContainer.Runtime.Unity
{
    internal sealed class StartableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IEnumerable<IStartable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public StartableLoopItem(
            IEnumerable<IStartable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            foreach (var x in this.entries)
            {
                try
                {
                    x.Start();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return false;
        }

        public void Dispose() => this.disposed = true;
    }
    
    internal sealed class PostStartableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IEnumerable<IPostStartable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public PostStartableLoopItem(
            IEnumerable<IPostStartable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            foreach (var x in this.entries)
            {
                try
                {
                    x.PostStart();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return false;
        }

        public void Dispose() => this.disposed = true;
    }
    
    internal sealed class FixedTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IReadOnlyList<IFixedTickable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public FixedTickableLoopItem(
            IReadOnlyList<IFixedTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            for (var i = 0; i < this.entries.Count; i++)
            {
                try
                {
                    this.entries[i].FixedTick();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return !this.disposed;
        }

        public void Dispose() => this.disposed = true;
    }

    internal sealed class PostFixedTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IReadOnlyList<IPostFixedTickable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public PostFixedTickableLoopItem(
            IReadOnlyList<IPostFixedTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            for (var i = 0; i < this.entries.Count; i++)
            {
                try
                {
                    this.entries[i].PostFixedTick();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return !this.disposed;
        }

        public void Dispose() => this.disposed = true;
    }

    internal sealed class TickableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IReadOnlyList<ITickable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public TickableLoopItem(
            IReadOnlyList<ITickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            for (var i = 0; i < this.entries.Count; i++)
            {
                try
                {
                    this.entries[i].Tick();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return !this.disposed;
        }

        public void Dispose() => this.disposed = true;
    }

    internal sealed class PostTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IReadOnlyList<IPostTickable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public PostTickableLoopItem(
            IReadOnlyList<IPostTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            for (var i = 0; i < this.entries.Count; i++)
            {
                try
                {
                    this.entries[i].PostTick();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return !this.disposed;
        }

        public void Dispose() => this.disposed = true;
    }

    internal sealed class LateTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IReadOnlyList<ILateTickable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public LateTickableLoopItem(
            IReadOnlyList<ILateTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            for (var i = 0; i < this.entries.Count; i++)
            {
                try
                {
                    this.entries[i].LateTick();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return !this.disposed;
        }

        public void Dispose() => this.disposed = true;
    }

    internal sealed class PostLateTickableLoopItem : IPlayerLoopItem, IDisposable
    {
        private readonly IReadOnlyList<IPostLateTickable> entries;
        private readonly EntryPointExceptionHandler exceptionHandler;
        private bool disposed;

        public PostLateTickableLoopItem(
            IReadOnlyList<IPostLateTickable> entries,
            EntryPointExceptionHandler exceptionHandler)
        {
            this.entries = entries;
            this.exceptionHandler = exceptionHandler;
        }

        public bool MoveNext()
        {
            if (this.disposed) return false;
            for (var i = 0; i < this.entries.Count; i++)
            {
                try
                {
                    this.entries[i].PostLateTick();
                }
                catch (Exception ex)
                {
                    if (this.exceptionHandler == null) throw;
                    this.exceptionHandler.Publish(ex);
                }
            }
            return !this.disposed;
        }

        public void Dispose() => this.disposed = true;
    }
}