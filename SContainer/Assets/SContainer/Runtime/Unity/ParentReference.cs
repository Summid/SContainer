using System;
using UnityEngine;

namespace SContainer.Runtime.Unity
{
    [Serializable]
    public struct ParentReference : ISerializationCallbackReceiver
    {
        [SerializeField]
        public string TypeName;

        [NonSerialized]
        public LifetimeScope Object;
        
        public Type Type { get; private set; }

        private ParentReference(Type type)
        {
            this.Type = type;
            this.TypeName = type.FullName;
            this.Object = null;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.TypeName = this.Type?.FullName;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(this.TypeName))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    this.Type = assembly.GetType(this.TypeName);
                    if (this.Type != null)
                        break;
                }
            }
        }

        public static ParentReference Create<T>() where T : LifetimeScope
        {
            return new ParentReference(typeof(T));
        }
    }
}