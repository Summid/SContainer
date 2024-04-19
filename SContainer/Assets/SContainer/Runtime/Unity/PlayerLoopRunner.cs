using System;
using System.Collections.Generic;
using System.Threading;

namespace SContainer.Runtime.Unity
{
    internal interface IPlayerLoopItem
    {
        bool MoveNext();
    }
    
    internal sealed class PlayerLoopRunner
    {
        private readonly Queue<IPlayerLoopItem> runningQueue = new Queue<IPlayerLoopItem>();
        private readonly Queue<IPlayerLoopItem> waitingQueue = new Queue<IPlayerLoopItem>();

        private readonly object runningGate = new object();
        private readonly object waitingGate = new object();

        private int running;

        public void Dispatch(IPlayerLoopItem item)
        {
            if (Interlocked.CompareExchange(ref this.running, 1, 1) == 1)
            {
                lock (this.waitingGate)
                {
                    this.waitingQueue.Enqueue(item);
                    return;
                }
            }

            lock (this.runningGate)
            {
                this.runningQueue.Enqueue(item);
            }
        }

        public void Run()
        {
            Interlocked.Exchange(ref this.running, 1);

            lock (this.runningGate)
            lock (this.waitingGate)
            {
                while (this.waitingQueue.Count > 0)
                {
                    var waitingItem = this.waitingQueue.Dequeue();
                    this.runningQueue.Enqueue(waitingItem);
                }
            }

            IPlayerLoopItem item;
            lock (this.runningGate)
            {
                item = this.runningQueue.Count > 0 ? this.runningQueue.Dequeue() : null;
            }

            while (item != null)
            {
                var continuous = false;
                try
                {
                    continuous = item.MoveNext();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }

                if (continuous)
                {
                    lock (this.waitingGate)
                    {
                        this.waitingQueue.Enqueue(item);
                    }
                }

                lock (this.runningGate)
                {
                    item = this.runningQueue.Count > 0 ? this.runningQueue.Dequeue() : null;
                }
            }

            Interlocked.Exchange(ref this.running, 0);
        }
    }
}