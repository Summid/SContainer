using System;
using System.Threading;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace SContainer.Runtime.Unity
{
    public struct SContainerInitialization { }
    public struct SContainerPostInitialization { }
    public struct SContainerStartup { }
    public struct SContainerPostStartup { }
    public struct SContainerFixedUpdate { }
    public struct SContainerPostFixedUpdate { }
    public struct SContainerUpdate { }
    public struct SContainerPostUpdate { }
    public struct SContainerLateUpdate { }
    public struct SContainerPostLateUpdate { }

    internal enum PlayerLoopTiming
    {
        Initialization = 0,
        PostInitialization = 1,

        Startup = 2,
        PostStartup = 3,

        FixedUpdate = 4,
        PostFixedUpdate = 5,

        Update = 6,
        PostUpdate = 7,

        LateUpdate = 8,
        PostLateUpdate = 9,
    }
    
    internal static class PlayerLoopHelper
    {
        private static readonly PlayerLoopRunner[] Runners = new PlayerLoopRunner[10];
        private static long initialized;

        public static void EnsureInitialized()
        {
            if (Interlocked.CompareExchange(ref initialized, 1, 0) != 0)
                return;

            for (var i = 0; i < Runners.Length; i++)
            {
                Runners[i] = new PlayerLoopRunner();
            }

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            var copyList = playerLoop.subSystemList;

            ref var initializeSystem = ref FindSubSystem(typeof(Initialization), copyList);
            InsertSubsystem(
                ref initializeSystem,
                null,
                new PlayerLoopSystem
                {
                    type = typeof(SContainerInitialization),
                    updateDelegate = Runners[(int)PlayerLoopTiming.Initialization].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(SContainerPostInitialization),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostInitialization].Run
                });
            
            ref var earlyUpdateSystem = ref FindSubSystem(typeof(EarlyUpdate), copyList);
            InsertSubsystem(
                ref earlyUpdateSystem,
                typeof(EarlyUpdate.ScriptRunDelayedStartupFrame),
                new PlayerLoopSystem
                {
                    type = typeof(SContainerStartup),
                    updateDelegate = Runners[(int)PlayerLoopTiming.Startup].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(SContainerPostStartup),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostStartup].Run
                });
            
            ref var fixedUpdateSystem = ref FindSubSystem(typeof(FixedUpdate), copyList);
            InsertSubsystem(
                ref fixedUpdateSystem,
                typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
                new PlayerLoopSystem
                {
                    type = typeof(SContainerFixedUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.FixedUpdate].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(SContainerPostFixedUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostFixedUpdate].Run
                });
            
            ref var updateSystem = ref FindSubSystem(typeof(Update), copyList);
            InsertSubsystem(
                ref updateSystem,
                typeof(Update.ScriptRunBehaviourUpdate),
                new PlayerLoopSystem
                {
                    type = typeof(SContainerUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.Update].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(SContainerPostUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostUpdate].Run
                });
            
            ref var lateUpdateSystem = ref FindSubSystem(typeof(PreLateUpdate), copyList);
            InsertSubsystem(
                ref lateUpdateSystem,
                typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
                new PlayerLoopSystem
                {
                    type = typeof(SContainerLateUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.LateUpdate].Run
                },
                new PlayerLoopSystem
                {
                    type = typeof(SContainerPostLateUpdate),
                    updateDelegate = Runners[(int)PlayerLoopTiming.PostLateUpdate].Run
                });

            playerLoop.subSystemList = copyList;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void Dispatch(PlayerLoopTiming timing, IPlayerLoopItem item)
        {
            EnsureInitialized();
            Runners[(int)timing].Dispatch(item);
        }

        private static ref PlayerLoopSystem FindSubSystem(Type targetType, PlayerLoopSystem[] systems)
        {
            for (var i = 0; i < systems.Length; i++)
            {
                if (systems[i].type == targetType)
                    return ref systems[i];
            }
            throw new InvalidOperationException($"{targetType.FullName} not in system");
        }

        private static void InsertSubsystem(
            ref PlayerLoopSystem parentSystem,
            Type beforeType,
            PlayerLoopSystem newSystem,
            PlayerLoopSystem newPostSystem)
        {
            var source = parentSystem.subSystemList;
            var insertIndex = -1;
            if (beforeType == null)
            {
                insertIndex = 0;
            }
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i].type == beforeType)
                {
                    insertIndex = i;
                }
            }

            if (insertIndex < 0)
            {
                throw new ArgumentException($"{beforeType.FullName} not in system {parentSystem} {parentSystem.type.FullName}");
            }

            var dest = new PlayerLoopSystem[source.Length + 2];
            for (var i = 0; i < dest.Length; i++)
            {
                if (i == insertIndex)
                {
                    dest[i] = newSystem;
                }
                else if (i == dest.Length - 1)
                {
                    dest[i] = newPostSystem;
                }
                else if (i < insertIndex)
                {
                    dest[i] = source[i];
                }
                else
                {
                    dest[i] = source[i - 1];
                }
            }

            parentSystem.subSystemList = dest;
        }
    }
}