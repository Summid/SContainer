using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    /// <summary>
    /// 字如其名，只返回 Container，即 IObjectResolver 对象；“每个作用域将会自动注册自身”功能会用到
    /// </summary>
    internal sealed class ContainerInstanceProvider : IInstanceProvider
    {
        public static readonly ContainerInstanceProvider Default = new ContainerInstanceProvider();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => resolver;
    }
}