using SContainer.Runtime.Annotations;
using SContainer.Runtime.Internal;
using System;
using System.Collections.Generic;

namespace SContainer.Runtime.Unity
{
    public sealed class EntryPointDispatcher : IDisposable
    {
        private readonly IObjectResolver container;
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        [Inject]
        public EntryPointDispatcher(IObjectResolver container)
        {
            this.container = container;
        }

        public void Dispatch()
        {
            PlayerLoopHelper.EnsureInitialized();

            EntryPointExceptionHandler exceptionHandler = null;
            try
            {
                exceptionHandler = this.container.Resolve<EntryPointExceptionHandler>();
            }
            catch (SContainerException ex) when (ex.InvalidType == typeof(EntryPointExceptionHandler))
            {
            }

            var initializables = this.container.Resolve<ContainerLocal<IReadOnlyList<IInitializable>>>().Value;
            for (var i = 0; i < initializables.Count; i++)
            {
                try
                {
                    initializables[i].Initialize();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        UnityEngine.Debug.LogException(ex);
                }
            }

            var postInitializables = this.container.Resolve<ContainerLocal<IReadOnlyList<IPostInitializable>>>().Value;
            for (var i = 0; i < postInitializables.Count; i++)
            {
                try
                {
                    postInitializables[i].PostInitialize();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler.Publish(ex);
                    else
                        UnityEngine.Debug.LogException(ex);
                }
            }

            var startables = this.container.Resolve<ContainerLocal<IReadOnlyList<IStartable>>>().Value;
            if (startables.Count > 0)
            {
                var loopItem = new StartableLoopItem(startables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Startup, loopItem);
            }
            
            var postStartables = this.container.Resolve<ContainerLocal<IReadOnlyList<IPostStartable>>>().Value;
            if (postStartables.Count > 0)
            {
                var loopItem = new PostStartableLoopItem(postStartables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostStartup, loopItem);
            }

            var fixedTickables = this.container.Resolve<ContainerLocal<IReadOnlyList<IFixedTickable>>>().Value;
            if (fixedTickables.Count > 0)
            {
                var loopItem = new FixedTickableLoopItem(fixedTickables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.FixedUpdate, loopItem);
            }

            var postFixedTickables = this.container.Resolve<ContainerLocal<IReadOnlyList<IPostFixedTickable>>>().Value;
            if (postFixedTickables.Count > 0)
            {
                var loopItem = new PostFixedTickableLoopItem(postFixedTickables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostFixedUpdate, loopItem);
            }

            var tickables = this.container.Resolve<ContainerLocal<IReadOnlyList<ITickable>>>().Value;
            if (tickables.Count > 0)
            {
                var loopItem = new TickableLoopItem(tickables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.Update, loopItem);
            }

            var postTickables = this.container.Resolve<ContainerLocal<IReadOnlyList<IPostTickable>>>().Value;
            if (postTickables.Count > 0)
            {
                var loopItem = new PostTickableLoopItem(postTickables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostUpdate, loopItem);
            }

            var lateTickables = this.container.Resolve<ContainerLocal<IReadOnlyList<ILateTickable>>>().Value;
            if (lateTickables.Count > 0)
            {
                var loopItem = new LateTickableLoopItem(lateTickables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.LateUpdate, loopItem);
            }

            var postLateTickables = this.container.Resolve<ContainerLocal<IReadOnlyList<IPostLateTickable>>>().Value;
            if (postLateTickables.Count > 0)
            {
                var loopItem = new PostLateTickableLoopItem(postLateTickables, exceptionHandler);
                this.disposable.Add(loopItem);
                PlayerLoopHelper.Dispatch(PlayerLoopTiming.PostLateUpdate, loopItem);
            }
        }

        public void Dispose() => this.disposable.Dispose();
    }
}