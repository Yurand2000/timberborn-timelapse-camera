using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;

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
