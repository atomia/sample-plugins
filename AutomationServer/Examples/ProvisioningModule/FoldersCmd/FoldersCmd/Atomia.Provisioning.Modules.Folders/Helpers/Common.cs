using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atomia.Provisioning.Base.Module;
using Atomia.Provisioning.Base;
using System.Web.Script.Serialization;
using Atomia.Provisioning.Modules.Common;

namespace Atomia.Provisioning.Modules.Folders.Helpers
{

    public class Common
    {

        public static ModuleService ReadAndFillServiceData(string serviceParamsSerialized, string serviceName)
        {

            ModuleService service = new ModuleService(serviceName, "");

            string[] paramsSplitted = serviceParamsSerialized.Split(',');

            for (int i = 0; i < paramsSplitted.Length; i++)
            {
                string oneParam = paramsSplitted[i];
                if (i == 0)
                {
                    oneParam = oneParam.Substring(1, oneParam.Length - 1); //remove first "{"
                }
                if (i == paramsSplitted.Length - 1)
                {
                    oneParam = oneParam.Substring(0, oneParam.Length - 1); //remove last "}"
                }

                string[] oneParamSplitted = oneParam.Split(":".ToArray(), 2, StringSplitOptions.None);
                string paramName = oneParamSplitted[0].ToString().Replace("\"", "");
                string paramValue = oneParamSplitted[1].ToString().Replace("\"", "");
                service.Properties.Add(new ModuleServiceProperty(paramName, paramValue));

            }

            return service;

        }

        public static ResourceDescription ReadAndFillResourceData(string resourceParamsSerialized, string serviceName)
        {

            ResourceDescription resource = new ResourceDescription(serviceName, "ResourceDescriptionFile");

            string[] paramsSplitted = resourceParamsSerialized.Split(',');

            for (int i = 0; i < paramsSplitted.Length; i++)
            {
                string oneParam = paramsSplitted[i];
                if (i == 0)
                {
                    oneParam = oneParam.Substring(1, oneParam.Length - 1); //remove first "{"
                }
                if (i == paramsSplitted.Length - 1)
                {
                    oneParam = oneParam.Substring(0, oneParam.Length - 1); //remove last "}"
                }
                if (oneParam != "")
                {
                    string[] oneParamSplitted = oneParam.Split(":".ToArray(), 2, StringSplitOptions.None);
                    string paramName = oneParamSplitted[0].ToString().Replace("\"", "");
                    string paramValue = oneParamSplitted[1].ToString().Replace("\"", "");
                    resource[paramName] = paramValue;
                }
            }

            return resource;

        }

        public static string SerializeService(ModuleService service)
        {

            Dictionary<string, Dictionary<string, string>> serviceFull = new Dictionary<string, Dictionary<string, string>>();

            bool bLoop = true;
            ModuleService serviceWorkingOn = service;
            string serviceKey = "current";
            while (bLoop)
            {
                Dictionary<string, string> serviceForSerialization = new Dictionary<string, string>();

                //serviceForSerialization.Add("\"" + "serviceMetaData_InstanceId" + "\"", "\"" + serviceWorkingOn.InstanceId + "\"");
                //serviceForSerialization.Add("\"" + "serviceMetaData_LogicalID" + "\"", "\"" + serviceWorkingOn.LogicalID + "\"");
                //serviceForSerialization.Add("\"" + "serviceMetaData_Name" + "\"", "\"" + serviceWorkingOn.Name + "\"");
                //serviceForSerialization.Add("\"" + "serviceMetaData_PhysicalID" + "\"", "\"" + serviceWorkingOn.PhysicalID + "\"");
                //serviceForSerialization.Add("\"" + "serviceMetaData_ProvisioningId" + "\"", "\"" + serviceWorkingOn.ProvisioningId + "\"");

                serviceForSerialization.Add("serviceMetaData_InstanceId", serviceWorkingOn.InstanceId);
                serviceForSerialization.Add("serviceMetaData_LogicalID", serviceWorkingOn.LogicalID);
                serviceForSerialization.Add("serviceMetaData_Name", serviceWorkingOn.Name);
                serviceForSerialization.Add("serviceMetaData_PhysicalID", serviceWorkingOn.PhysicalID);
                serviceForSerialization.Add("serviceMetaData_ProvisioningId", serviceWorkingOn.ProvisioningId);

                foreach (ModuleServiceProperty prop in serviceWorkingOn.Properties)
                {
                    if (prop.Value == null)
                    {
                        //serviceForSerialization.Add("\"" + prop.Name + "\"", "\"\"");
                        serviceForSerialization.Add(prop.Name, "");
                    }
                    else
                    {
                        //serviceForSerialization.Add("\"" + prop.Name + "\"", "\"" + prop.Value.Replace(" ", "_#_#_") + "\"");
                        //serviceForSerialization.Add(prop.Name, prop.Value.Replace(" ", "_#_#_"));
                        serviceForSerialization.Add(prop.Name, prop.Value);
                    }
                }

                //serviceFull.Add("\"" + serviceKey + "\"", serviceForSerialization);
                serviceFull.Add(serviceKey, serviceForSerialization);

                if (serviceWorkingOn.Parent == null)
                {
                    bLoop = false;
                }
                else
                {
                    serviceKey = serviceKey + ".parent";
                    serviceWorkingOn = serviceWorkingOn.Parent;
                }
            }

            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            string jsonSerializedFullService = oSerializer.Serialize(serviceFull);
            //jsonSerializedFullService = jsonSerializedFullService.Replace(" ", "_#_#_");

            return jsonSerializedFullService;
        }

        public static ModuleService DeserializeService(string serviceSerialized)
        {

            ModuleService result = null;
            //serviceSerialized = serviceSerialized.Replace("_#_#_", " ");

            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            Dictionary<string, Dictionary<string, string>> serviceFull = (Dictionary<string, Dictionary<string, string>>)oSerializer.Deserialize(serviceSerialized, typeof(Dictionary<string, Dictionary<string, string>>));

            string serviceKey = "current";
            ModuleService lastService = null;

            for (int i = 0; i < serviceFull.Count; i++)
            {

                Dictionary<string, string> oneServiceDict = serviceFull[serviceKey];

                ModuleService tempService = new ModuleService(oneServiceDict["serviceMetaData_Name"], oneServiceDict["serviceMetaData_PhysicalID"]);
                tempService.InstanceId = oneServiceDict["serviceMetaData_InstanceId"];
                //tempService.LogicalID = oneServiceDict["serviceMetaData_LogicalID"]; readonly
                //tempService.Name = oneServiceDict["serviceMetaData_Name"]; readonly
                //tempService.PhysicalID = oneServiceDict["serviceMetaData_PhysicalID"]; set in constructor
                tempService.ProvisioningId = oneServiceDict["serviceMetaData_ProvisioningId"];

                foreach (KeyValuePair<String, String> entry in oneServiceDict)
                {
                    if (entry.Key.IndexOf("serviceMetaData_") < 0)
                    {
                        tempService.Properties.Add(new ModuleServiceProperty(entry.Key, entry.Value));
                    }
                }

                if (serviceKey == "current")
                {
                    lastService = tempService;
                    result = lastService;
                }
                else
                {
                    lastService.Parent = tempService;
                    lastService = lastService.Parent;
                }
                serviceKey = serviceKey + ".parent";

            }

            return result;

        }

        public static Dictionary<string, ResourceDescription> GetResourcesForModulePlugin(string moduleName)
        {

            Dictionary<string, ResourceDescription> resources = new Dictionary<string, ResourceDescription>();

            string resourcesFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace("Modules\\", "");
            resourcesFilePath = resourcesFilePath + "resources.xml";

            //for testing from code this path must be set through code
            //resourcesFilePath = "c:\\Program Files (x86)\\Atomia\\AutomationServer\\Common\\resources.xml";

            System.Xml.Linq.XDocument xmlDoc = null;

            try
            {
                xmlDoc = System.Xml.Linq.XDocument.Load(resourcesFilePath);
            }
            catch (Exception ex)
            {
                throw ExceptionHelper.GetModuleException("ID422004", null, ex);
            }

            if (xmlDoc == null)
            {
            }
            else
            {
                foreach (System.Xml.Linq.XElement resourceAndPolicy in xmlDoc.Root.Elements("bindings"))
                {
                    resources = FillBindingsElement(resourceAndPolicy, moduleName);
                }
            }

            return resources;

        }

        private static Dictionary<string, ResourceDescription> FillBindingsElement(System.Xml.Linq.XElement resourceAndPolicy, string moduleName)
        {

            Dictionary<string, ResourceDescription> resources = new Dictionary<string, ResourceDescription>();

            // find all resources descriptions for module
            foreach (System.Xml.Linq.XElement module in resourceAndPolicy.Element("moduleList").Elements())
            {
                if (module.Attribute("name").Value == moduleName)
                {

                    foreach (System.Xml.Linq.XElement resourceItem in resourceAndPolicy.Element("resourceList").Elements())
                    {

                        ResourceDescription resourceDesc = new ResourceDescription(resourceItem.Attribute("name").Value, moduleName);

                        resourceDesc.ShouldBeLocked = true;

                        foreach (System.Xml.Linq.XElement propElem in resourceItem.Elements("property"))
                        {
                            resourceDesc[propElem.Attribute("name").Value] = propElem.Value;
                        }

                        foreach (System.Xml.Linq.XElement propListEl in resourceItem.Elements("propertyList"))
                        {
                            ResourceDescriptionPropertyList resDescPropList = new ResourceDescriptionPropertyList();
                            resDescPropList.PropertyListName = propListEl.Attribute("name").Value;
                            foreach (System.Xml.Linq.XElement propListItem in propListEl.Elements("propertyListItem"))
                            {
                                resDescPropList.PropertyListItems.Add(propListItem.Value);
                            }

                            resourceDesc.PropertyList.Add(resDescPropList);
                        }

                        // first check if this resource already exist in list of resoources
                        if (resources.ContainsKey(resourceDesc.Name))
                        {
                            // check if they are the same
                            if (resourceDesc.Equals(resources[resourceDesc.Name]))
                            {
                                resourceDesc = resources[resourceDesc.Name];
                            }
                            else
                            {
                                // they have the same name but settings are different.
                                // this is not allowed
                                throw ExceptionHelper.GetModuleException("ID422005", new Dictionary<string, string>() { { "Message", resourceDesc.Name } }, null);
                            }
                        }
                        else
                        {
                            resources[resourceDesc.Name] = resourceDesc;
                        }

                    }

                }
            }

            return resources;

        }

    }

}
