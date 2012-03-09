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
        
        serviceFull = json.loads(serviceSerialized)
        
        return serviceFull
    
        
    def SerializeService(self, service):
        
        jsonSerializedService = json.dumps(service)

        return jsonSerializedService;
        
