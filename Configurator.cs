using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;

namespace Yurand.Timberborn.TimelapseCamera
{
    [Configurator(SceneEntrypoint.InGame)]
    public class Configurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ScreenshotService>().AsSingleton();
            containerDefinition.Bind<ScreenshotSunService>().AsSingleton();
            containerDefinition.Bind<TimelapseManager>().AsSingleton();
        }
    }
}
