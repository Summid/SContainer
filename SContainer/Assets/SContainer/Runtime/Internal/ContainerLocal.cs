using SContainer.Runtime.Annotations;

namespace SContainer.Runtime.Internal
{
    public sealed class ContainerLocal<T>
    {
        public readonly T Value;

        [Inject]
        public ContainerLocal(T value)
        {
            this.Value = value;
        }
    }
}