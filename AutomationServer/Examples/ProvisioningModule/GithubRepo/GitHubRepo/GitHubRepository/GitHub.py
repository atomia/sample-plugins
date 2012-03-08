'''
Created on 02.03.2012.

@author: ivras
'''

import sys
import atomia_entities
import string
import common
import GitHubRepositoryCommand
    
class GitHub(object):

    def __init__(self):pass
    
    common = common.common()        
    transactionContext = 'NoTransaction'
    commands = list()
    
    startArgIndex = 1
    argsLength = 0
    
    serviceFull = None    
    serviceOldFull = None
    serviceNewFull = None
        
    def GetModuleCommand(self, service, newService, resource, commandType, listDepth):

        command = None
        serviceName = service.Name

        command_type = None
        
        if serviceName == 'GitHubRepository':
            command = GitHubRepositoryCommand.GitHubRepositoryCommand()
            command.CommandType = command_type
            command.Service = service
            command.Resource = resource
            if commandType == 'Modify':
                command.NewService = newService
            command.CommandType = commandType
            command.ListDepth = listDepth
            command.TransactionContext = self.transactionContext
        
        return command
        
    
    def PrepareCommandList(self, service, newServiceSettings, resource, commandType):
        
        if commandType != 'Remove':
            self.commands.append(self.GetModuleCommand(service, newServiceSettings, resource, commandType, -1));

        if service.Children != None:
            i = 0
            for childService in service.Children:
                newChildSettings = None;
                if newServiceSettings != None and newServiceSettings.Children != None and newServiceSettings.Children.Count > i:
                    newChildSettings = newServiceSettings.Children[i];
                
                self.PrepareCommandList(childService, newChildSettings, resource, commandType);
                i = i + 1
            
        if commandType == 'Remove':        
            self.commands.append(self.GetModuleCommand(service, newServiceSettings, resource, commandType, -1))
        
   
    def PrepareAndExecuteCommandsFromList(self):
        
        for command in self.commands:        
            command.Prepare()        

        try:            
            for command in self.commands:            
                command.Execute()
        except:
            self.RollbackTransaction()
            raise

        
    '''methods which are called by AutomathedServer and CmdLocal - START'''
    
    def BeginTransaction(self):
        if len(self.commands) > 0:            
            raise 'Current transaction is not over.'
        
    
    def CallOperation(self, service, operationName, operationArgument, resource):
        command = self.GetModuleCommand(service, None, resource, 'CallOperation', -1)
        return command.CallOperation(operationName, operationArgument)
        
        
    def CommitTransaction(self):
        for command in self.commands:            
            command.CleanUp();
            self.commands = list()
        
    
    def GetModuleServiceDescription(self):        
        return 'Atomia.Provisioning.Modules.GitHub.ServiceDescription.xml';
    
    
    def ListServices(self, service, maxDepth, resource):        
        command = self.GetModuleCommand(service, None, resource, 'List', maxDepth);
        command.Execute();
        return service;
    
    
    def ListServicesNoChildren(self, serviceName, resource):
        raise 'This feature is not implemented.'
    
    
    def ModifyService(self, service, newService, resource):        
        self.PrepareCommandList(service, newService, resource, 'Modify');
        self.PrepareAndExecuteCommandsFromList();
        
    
    def MoveToResource(self, service, currentResource, targetResource):        
        raise 'This feature is not implemented.'
        
    
    def ProvideService(self, service, resource):        
        self.PrepareCommandList(service, None, resource, 'Add')
        self.PrepareAndExecuteCommandsFromList()
        
    
    def RemoveService(self, service, resource):        
        self.PrepareCommandList(service, None, resource, 'Remove')
        self.PrepareAndExecuteCommandsFromList()
        
        
    def RollbackTransaction(self):
        
        rollback_exception = None
        size = len(self.commands)
        
        i = size - 1
        bLoop = True
        
        while bLoop:
            try:            
                if self.commands[i].Status == 'Executed':                
                    self.commands[i].Undo();                

                self.commands[i].CleanUp();
            
            except Exception, e:
                rollback_exception = e
            
            i = i - 1
            if i < 0:
                bLoop = False
            

        self.commands = list()

        if rollback_exception != None:
            raise rollback_exception
        
        
        
    '''methods which are called by AutomathedServer and CmdLocal - END '''
        
    def HandleAddRemove(self, serviceName, commandType):
        
        print 'HandleAddRemove'
        ''' Check args number '''
        if not(self.argsLength == self.startArgIndex + 5 or self.argsLength == self.startArgIndex + 6):
            raise 'Wrong number of arguments. 5 or 6 arguments expected'
                
        resource = self.common.ReadAndFillResourceData(sys.argv[self.startArgIndex + 3], serviceName)
        service = self.common.ReadAndFillServiceData(sys.argv[self.startArgIndex + 4], serviceName)
        
        if self.argsLength == self.startArgIndex + 6:
            ''' Read and fill full serialized service '''                        
            self.serviceFull = self.common.DeserializeService(sys.argv[self.startArgIndex + 5])
                        
        if commandType == 'Add':                                
            self.ProvideService(service, resource)
        elif commandType == 'Remove':                                
            self.RemoveService(service, resource)
        
        ''' After add we should update service properties if any is changed, if StdinStdOut is true '''
        if self.serviceFull != None:             
            if string.lower(self.serviceFull["StdinStdout"]) == "true":
                            
                for prop in service.Properties:
                    self.serviceFull[prop.Name] = prop.Value
                
                jsonSerializedFullService = self.common.SerializeService(self.serviceFull)
                                
                bLoop = True
                sCurrentMessagePart = ''
                sRestOfMessage = jsonSerializedFullService
                while bLoop:
                    if sRestOfMessage.Length < 1024:
                        sCurrentMessagePart = sRestOfMessage
                        bLoop = False
                    else:
                        sCurrentMessagePart = sRestOfMessage[0, 1023];
                        sRestOfMessage = string.replace(sRestOfMessage, sCurrentMessagePart, "")

                    print sCurrentMessagePart



    def HandleModify(self, serviceName):
        print 'HandleModify'
            
        ''' Check args number '''
        if not(self.argsLength == self.startArgIndex + 6 or self.argsLength == self.startArgIndex + 8):
            raise 'Wrong number of arguments. 6 or 8 arguments expected'
                        
        resource = self.common.ReadAndFillResourceData(sys.argv[self.startArgIndex + 3], serviceName)
        serviceOld = self.common.ReadAndFillServiceData(sys.argv[self.startArgIndex + 4], serviceName)
        serviceNew = self.common.ReadAndFillServiceData(sys.argv[self.startArgIndex + 5], serviceName)
    
        if len(sys.argv) == self.startArgIndex + 8:        
            self.serviceOldFull = self.common.DeserializeService(sys.argv[self.startArgIndex + 6])
            self.serviceNewFull = self.common.DeserializeService(sys.argv[self.startArgIndex + 7])
                        
    
        self.ModifyService(serviceOld, serviceNew, resource)
    
        
        if self.serviceNewFull != None:
        
            if self.serviceNewFull["StdinStdout"].ToLower() == "true":
            
                for prop in serviceNew.Properties:
                    self.serviceNewFull[prop.Name] = prop.Value
                
                jsonSerializedFullService = self.common.SerializeService(self.serviceNewFull)

                bLoop = True
                sCurrentMessagePart = ''
                sRestOfMessage = jsonSerializedFullService
                while bLoop:
                
                    if sRestOfMessage.Length < 1024:                    
                        sCurrentMessagePart = sRestOfMessage
                        bLoop = False
                    else:
                        sCurrentMessagePart = sRestOfMessage[0, 1023]
                        sRestOfMessage = string.replace(sRestOfMessage, sCurrentMessagePart, "")

                    print sCurrentMessagePart

    
    
    def HandleCallOperation(self, serviceName):        

        ''' region Check args number '''
        if len(sys.argv) != self.startArgIndex + 6:                        
            raise 'Wrong number of arguments. 6 arguments expected'
    
        operationName = '';
        operationArgument = '';

        oneParam = sys.argv[self.startArgIndex + 2]
        
        oneParamSplitted = string.split(oneParam, ':', 1)
        
        if len(oneParamSplitted) != 2:        
            raise 'Command arguments missing.'
        else:        
            operationName = oneParamSplitted[1]
        
        oneParam = sys.argv[self.startArgIndex + 3]
        oneParamSplitted = string.split(oneParam, ":", 1)
        
        if len(oneParamSplitted) != 2:
            raise 'Command arguments missing.'
        else:        
            operationArgument = oneParamSplitted[1]        

        resource = self.common.ReadAndFillResourceData(sys.argv[self.startArgIndex + 4], serviceName)
        service = self.common.ReadAndFillServiceData(sys.argv[self.startArgIndex + 5], serviceName)

        operationResult = self.CallOperation(service, operationName, operationArgument, resource)
        
        bLoop = True
        sCurrentMessagePart = ''
        sRestOfMessage = operationResult
        
        while bLoop:        
            if len(sRestOfMessage) < 1024:            
                sCurrentMessagePart = sRestOfMessage
                bLoop = False            
            else:            
                sCurrentMessagePart = sRestOfMessage[0, 1023]
                sRestOfMessage = string.replace(sRestOfMessage, sCurrentMessagePart, "")

            print sCurrentMessagePart        
        
    
    
    def HandleAndProvide(self):        
        
        self.argsLength = len(sys.argv)
    
        if self.argsLength < self.startArgIndex + 3:        
            raise Exception('error: not all needed arguments are supplied!')
            
        serviceName = sys.argv[self.startArgIndex + 0]
        commandType = '';
        
        if sys.argv[self.startArgIndex + 1] == 'CallOperation':            
            commandType = sys.argv[self.startArgIndex + 1]
        else:
            self.transactionContext = string.replace(sys.argv[self.startArgIndex + 1], "$", "")
            commandType = sys.argv[self.startArgIndex + 2]
    
        service = atomia_entities.ModuleService()
        service.Name = serviceName
        serviceOld = atomia_entities.ModuleService()
        serviceOld.Name = serviceName
        serviceNew = atomia_entities.ModuleService()
        serviceNew.Name = serviceName    
    
        
        
        resource = atomia_entities.ResourceDescription()
        resource.Name = serviceName
        resource.ManagerName = "ResourceDescriptionFile"
        
        if commandType == 'Add' or commandType == 'Remove':
            self.HandleAddRemove(serviceName, commandType)
        elif commandType == 'Modify':
            self.HandleModify(serviceName)
        elif commandType == 'CallOperation':
            self.HandleCallOperation(serviceName)
