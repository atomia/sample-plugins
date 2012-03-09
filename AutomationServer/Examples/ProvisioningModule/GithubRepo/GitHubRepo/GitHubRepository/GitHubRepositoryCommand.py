'''
Created on 05.03.2012.

@author: ivras
'''

import github2.client

class GitHubRepositoryCommand(object):

       
    def __init__(self, command_type=None, service=None, resource=None, newService=None, listDepth=None, transactionContext=None, status=None): 
        self.CommandType = command_type
        self.Service = service
        self.Resource = resource        
        self.NewService = newService            
        self.ListDepth = listDepth
        self.TransactionContext = transactionContext
        self.Status = status
        
    def ExecuteAdd(self):
        
        username = self.Resource.GetPropertyValue('UserName')
        api_token = self.Resource.GetPropertyValue('APIToken')
        gh = github2.client.Github(username, api_token)
        
        if self.TransactionContext == "BeginTransaction":            
            
            repoName = self.Service.GetPropertyValue('Name')
            repoDescription = self.Service.GetPropertyValue('Description')
            repoUrl = self.Service.GetPropertyValue('HomePageUrl')
            repoIsPublic = True            
            if self.Service.GetPropertyValue('IsPublic') != 'true':
                repoIsPublic = False
            
            createdRepo = gh.repos.create(repoName, repoDescription, repoUrl, repoIsPublic)
            
            self.Service.SetPropertyValue('Url', createdRepo.url)
                        
            return self.Service
            
        elif "CommitTransaction": pass
            
        elif "RollBackTransaction":            
            repoName = self.Service.GetPropertyValue('Name')
            gh.repos.delete(repoName)       
        
            
    def ExecuteModify(self):        
        
        username = self.Resource.GetPropertyValue('UserName')
        api_token = self.Resource.GetPropertyValue('APIToken')
        gh = github2.client.Github(username, api_token)
        
        newServiceRepoIsPublic = True
        oldServiceRepoIsPublic = True            
        if self.NewService.GetPropertyValue('IsPublic') != 'true':
            newServiceRepoIsPublic = False
        if self.Service.GetPropertyValue('IsPublic') != 'true':
            oldServiceRepoIsPublic = False
            
        
        repoName = self.Service.GetPropertyValue('Name')
        username = self.Resource.GetPropertyValue('UserName')
        descriptionNew = self.NewService.GetPropertyValue('Description')
        descriptionOld = self.Service.GetPropertyValue('Description')
        
        if self.TransactionContext == "BeginTransaction":
            
            if newServiceRepoIsPublic != oldServiceRepoIsPublic:
                if newServiceRepoIsPublic == True:
                    gh.repos.set_public(repoName)
                else:
                    gh.repos.set_private(repoName)
            
            gh.repos.set_description(repoName, descriptionNew, username)
            
            self.Service.SetPropertyValue('Description', descriptionNew)
            
        elif "CommitTransaction": pass
            
        elif "RollBackTransaction":            
            if newServiceRepoIsPublic != oldServiceRepoIsPublic:
                if oldServiceRepoIsPublic == True:
                    gh.repos.set_public(repoName)
                else:
                    gh.repos.set_private(repoName)
            
            gh.repos.set_description(repoName, descriptionOld, username)
            
            self.Service.SetPropertyValue('Description', descriptionOld)
                

    def ExecuteRemove(self):
        
        username = self.Resource.GetPropertyValue('UserName')
        api_token = self.Resource.GetPropertyValue('APIToken')
        gh = github2.client.Github(username, api_token)
        
        repoName = self.Service.GetPropertyValue('Name')
        
        if self.TransactionContext == "BeginTransaction": pass            
            
        elif "CommitTransaction":            
            gh.repos.delete(repoName)
            
        elif "RollBackTransaction": pass
            

        
    def Execute(self):
        
        try:
        
            if self.CommandType == 'Add':
                self.ExecuteAdd()
            elif self.CommandType == 'Modify':                
                self.ExecuteModify()                    
            elif self.CommandType == 'Remove':
                self.ExecuteRemove()                    
            else:
                raise 'Action ' + self.CommandType + ' is not implemented.'            

            self.Status = 'Executed'
        
        except Exception, e:
            raise e
        
        
        
    def ValidateService(self, moduleService): pass
        

    def CallOperation(self, operationName, operationArgument):
        
        result = '';        

        if operationName == "GetRepositoryUrl":                    
            result = self.Service.GetPropertyValue("Url")
        else:
            raise 'Called operation' + operationName + 'is not supported.' 
        
        return result;

        
    def Prepare(self): pass        
        
    def Clear(self): pass
    
    def CleanUp(self): pass
    
    def Undo(self): pass
