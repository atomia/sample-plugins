using System;
using Atomia.Provisioning.Base;
using Atomia.Provisioning.Base.Module;
using Atomia.Provisioning.Modules.Common;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

namespace Atomia.Provisioning.Modules.Haproxy.Commands
{
    class ListenerCommand : HaproxyCommandBase
    {
        public ListenerCommand(ModuleService service, ResourceDescription resource, ModuleService newServiceSettings, ModuleCommandType commandType, int listDepth)
            : base(service, resource, newServiceSettings, commandType, listDepth)
        {
        }

        protected override string GetResourceURL(ModuleService moduleService)
        {
            string id = moduleService.Properties.Single(p => p.Name == "Id").Value;
            return string.IsNullOrEmpty(id) ? "listener" : ("listener/" + id);
        }

        public override string CallOperation(string operationName, string operationArgument)
        {
            switch (operationName)
            {
                case "GetStats":
                    string json = this.REST_Execute_GET("stats");
                    var deserialized = this.jsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);
                    var returnlist = new List<Dictionary<string, string>>();
                    foreach (var csv_row in deserialized)
                    {
                        if (csv_row["pxname"] == this.service["Name"])
                        {
                            returnlist.Add(csv_row);
                        }
                    }
                    return jsonSerializer.Serialize(returnlist);

                default:
                    throw ExceptionHelper.GetModuleException("ID400019", null, null);
            }
        }
    }
}
