'''
Created on 02.03.2012.

@author: ivras
'''

class ModuleService(object):

    def __init__(self, Children=None, InstanceId=None, LogicalID=None, Name=None, Parent=None, PhysicalID=None, Properties=None, ProvisioningId=None):
        self.Children = Children
        self.InstanceId = InstanceId
        self.LogicalID = LogicalID
        self.Name = Name
        self.Parent = Parent
        self.PhysicalID = PhysicalID
        self.Properties = Properties
        self.ProvisioningId = ProvisioningId

    
    def GetPropertyValue(self, propertyName):
        
        propValue = ''
        
        for prop in self.Properties:
            if prop.Name == propertyName:
                propValue = prop.Value
                
        return propValue
    
    
    

class ResourceDescription(object):
    
    def __init__(self, ManagerName=None, Name=None, PropertyList=None, ShouldBeLocked=None):
        self.ManagerName = ManagerName
        self.Name = Name
        self.PropertyList = PropertyList
        self.ShouldBeLocked = ShouldBeLocked
    
    def GetPropertyValue(self, propertyName):
        
        propValue = ''
        
        for prop in self.PropertyList:
            if prop.Name == propertyName:
                propValue = prop.Value
                
        return propValue
    
    
    

class ServiceProperty(object):
    
    def __init__(self, Name=None, Value=None):
        self.Name = Name
        self.Value = Value
    


class ResourceProperty(object):
    
    def __init__(self, Name=None, Value=None):
        self.Name = Name
        self.Value = Value
