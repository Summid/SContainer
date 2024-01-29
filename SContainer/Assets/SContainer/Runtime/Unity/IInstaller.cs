namespace SContainer.Runtime.Unity
{
    /// <summary>
    /// 通过该接口向 Scope 添加额外的（注册）逻辑
    /// </summary>
    public interface IInstaller
    {
        void Install(IContainerBuilder builder);
    }
}