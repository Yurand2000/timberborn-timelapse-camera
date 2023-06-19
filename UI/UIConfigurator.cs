using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace Yurand.Timberborn.TimelapseCamera.UI
{
    [Configurator(SceneEntrypoint.InGame)]
    public class UIConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<TimelapsePanel>().AsSingleton();
            containerDefinition.Bind<TimelapseInterface>().AsSingleton();
        }
    }
}
