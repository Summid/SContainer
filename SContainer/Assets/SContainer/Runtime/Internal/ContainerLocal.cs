using SContainer.Runtime.Annotations;

namespace SContainer.Runtime.Internal
{
    /// <summary>
    /// 内部使用的包装工具类，包装类型 T 对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
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