namespace SContainer.Runtime
{
    /*
     * IInstanceProvider 类似访问者模式中的 Element 接口，IObjectResolver 就像 Visitor；
     * 这里的 provider 只需要在 SpawnInstance 方法中，用 IObjectResolver 解析出被装载的对象即可；
     * 当然不同的 provider，其解析的逻辑是不同的，使用 IObjectResolver 的方式也不同。
     */
    
    /// <summary>
    /// 从 <see cref="IObjectResolver"/> 中解析被装载的对象，不同的实现者有不同的处理方式
    /// </summary>
    public interface IInstanceProvider
    {
        object SpawnInstance(IObjectResolver resolver);
    }
}