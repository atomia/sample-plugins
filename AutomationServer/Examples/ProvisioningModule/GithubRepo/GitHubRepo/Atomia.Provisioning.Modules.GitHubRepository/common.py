'''
Created on 02.03.2012.

@author: ivras
'''

import string
import json
import atomia_entities

class common(object):

    def __init__(self): pass    
    
    def ReadAndFillResourceData(self, resourceParamsSerialized, serviceName):
        
        resource = atomia_entities.ResourceDescription()
        resource.Name = serviceName
        resource.ManagerName = "ResourceDescriptionFile"
        resource.PropertyList = list()

        paramsSplitted = string.split(resourceParamsSerialized, ',')
        
        i = 0
        for oneParam in paramsSplitted:            
            
            if i == 0:                
                ''' remove first "{" '''
                oneParam = oneParam[1: len(oneParam)] 
                
            if i == len(paramsSplitted) - 1:
                ''' remove last "}" '''
                oneParam = oneParam[0: len(oneParam) - 1]
                
            if oneParam != "":                
                oneParamSplitted = string.split(oneParam, ":", 1)
                paramName = oneParamSplitted[0].replace("\"", "")
                paramValue = oneParamSplitted[1].replace("\"", "")
                resourceProperty = atomia_entities.ResourceProperty()
                resourceProperty.Name = paramName
                resourceProperty.Value = paramValue
                resource.PropertyList.append(resourceProperty)                
                
            i = i + 1
        
        return resource;
        
        
    def ReadAndFillServiceData(self, serviceParamsSerialized, serviceName):
        
        service = atomia_entities.ModuleService()
        service.Name = serviceName
        service.Properties = list()
        
        paramsSplitted = string.split(serviceParamsSerialized, ',')

        i = 0
        for oneParam in paramsSplitted:
            
            if i == 0:
                ''' remove first "{" '''
                oneParam = oneParam[1: len(oneParam)] 
                
            if i == len(paramsSplitted) - 1:
                ''' remove last "}" '''
                oneParam = oneParam[0: len(oneParam) - 1]

            if oneParam != "":            
                oneParamSplitted = string.split(oneParam, ":", 1)
                paramName = oneParamSplitted[0].replace("\"", "")
                paramValue = oneParamSplitted[1].replace("\"", "")
                serviceProperty = atomia_entities.ServiceProperty()
                serviceProperty.Name = paramName
                serviceProperty.Value = paramValue
                service.Properties.append(serviceProperty)
            
            i = i + 1
            
        return service


    def DeserializeService(self, serviceSerialized):        
        
        result = None

        serviceFull = json.loads(serviceSerialized)
        
        '''serviceKey = "current";
        lastService = None;

        i = 0
        for service in serviceFull:        
            
            Dictionary < string, string > oneServiceDict = serviceFull[serviceKey];

                ModuleService tempService = new ModuleService(oneServiceDict["serviceMetaData_Name"], oneServiceDict["serviceMetaData_PhysicalID"]);
                tempService.InstanceId = oneServiceDict["serviceMetaData_InstanceId"];
                // tempService.LogicalID = oneServiceDict["serviceMetaData_LogicalID"]; readonly
                // tempService.Name = oneServiceDict["serviceMetaData_Name"]; readonly
                // tempService.PhysicalID = oneServiceDict["serviceMetaData_PhysicalID"]; set in constructor
                tempService.ProvisioningId = oneServiceDict["serviceMetaData_ProvisioningId"];

                foreach (KeyValuePair < String, String > entry in oneServiceDict)
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

        }'''
        
        return result
        
        
