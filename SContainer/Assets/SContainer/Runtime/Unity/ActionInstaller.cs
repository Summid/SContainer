using System;

namespace SContainer.Runtime.Unity
{
    public sealed class ActionInstaller : IInstaller
    {
        public static implicit operator ActionInstaller(Action<IContainerBuilder> installation)
            => new ActionInstaller(installation);
        
        private readonly Action<IContainerBuilder> configuration;

        public ActionInstaller(Action<IContainerBuilder> configuration)
        {
            this.configuration = configuration;
        }
        
        public void Install(IContainerBuilder builder)
        {
            this.configuration(builder);
        }
    }
}