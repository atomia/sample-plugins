using System;
using System.Collections.Generic;
using Atomia.Provisioning.Modules.Common;
using Atomia.Provisioning.Modules.Haproxy.Commands;

namespace Atomia.Provisioning.Modules.Haproxy
{
    class Haproxy : CommandPatternModuleBase
    {
        public override string GetServiceDescriptionAssemblyPath()
        {
            return "Atomia.Provisioning.Modules.Haproxy.ServiceDescription.xml";
        }

        public override Dictionary<string, Type> GetServiceNameToCommandClassTypeMapping()
        {
            return new Dictionary<string, Type>() { 
                { "HaproxyGlobal", typeof(GlobalCommand) },
                { "HaproxyDefault", typeof(DefaultCommand) },
                { "HaproxyListener", typeof(ListenerCommand) },
                { "HaproxyListenerSetting", typeof(ListenerSettingCommand) },
            };
        }
    }
}
