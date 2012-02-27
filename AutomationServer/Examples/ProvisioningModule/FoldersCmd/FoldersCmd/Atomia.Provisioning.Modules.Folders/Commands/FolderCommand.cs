using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atomia.Provisioning.Modules.Common;
using Atomia.Provisioning.Base;
using Atomia.Provisioning.Base.Module;
using System.IO;

namespace Atomia.Provisioning.Modules.Folders.Commands
{
    public class FolderCommand : ModuleCommandSimpleBase
    {

        private string transContext = string.Empty;

        public FolderCommand(ModuleService service, ResourceDescription resource, ModuleService newServiceSettings, ModuleCommandType commandType, int listDepth, string transContext)
            : base(service, resource, newServiceSettings, commandType, listDepth)
        {
            this.transContext = transContext;
        }

        protected override void ExecuteModify(ModuleService oldService, ModuleService newService)
        {
            switch (transContext)
            {
                case "BeginTransaction":
                    {
                        //we rename folder to new name
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFolderName = string.Empty;
                        string oldFolderName = string.Empty;

                        newFolderName = newService.Properties.Find(p => p.Name == "Name").Value;
                        oldFolderName = oldService.Properties.Find(p => p.Name == "Name").Value;
                        string newFolderFullPath = rootFolder + newFolderName;
                        string oldFolderFullPath = rootFolder + oldFolderName;

                        if (oldFolderName != newFolderName)
                        {
                            if (Directory.Exists(oldFolderFullPath))
                            {
                                if (Directory.Exists(newFolderFullPath))
                                {
                                    throw ExceptionHelper.GetModuleException("ID422010", null, null);
                                }
                                else
                                {
                                    Directory.Move(oldFolderFullPath, newFolderFullPath);
                                }
                            }
                            else
                            {
                                throw ExceptionHelper.GetModuleException("ID422007", null, null);
                            }
                        }
                    }
                    break;
                case "CommitTransaction":
                    {
                        //nothing to do folder is already renamed
                    }
                    break;
                case "RollBackTransaction":
                    {
                        //we should revert renamed folder name
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFolderName = string.Empty;
                        string oldFolderName = string.Empty;

                        newFolderName = newService.Properties.Find(p => p.Name == "Name").Value;
                        oldFolderName = oldService.Properties.Find(p => p.Name == "Name").Value;

                        string newFolderFullPath = rootFolder + newFolderName;
                        string oldFolderFullPath = rootFolder + oldFolderName;

                        if (newFolderName != oldFolderName)
                        {
                            //now we expect that folder with newFolderFUllPath exists, and we should revert its name to oldfolderfullpath
                            if (Directory.Exists(newFolderFullPath))
                            {
                                if (Directory.Exists(oldFolderFullPath))
                                {
                                    //nothing to do, old folder already exists - well this shouldn't happened
                                }
                                else
                                {
                                    Directory.Move(newFolderFullPath, oldFolderFullPath);
                                }
                            }
                            else
                            {
                                //nothing to do, folder don't exists and we don't have what to revert - well this shouldn't happened
                            }
                        }
                    }
                    break;
            }
            
            
        }

        protected override void ExecuteAdd(Base.Module.ModuleService moduleService)
        {
            
            switch(transContext)
            {
                case "BeginTransaction":
                    {
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFolderName = moduleService["Name"].ToString();

                        if (!Directory.Exists(rootFolder))
                        {
                            Directory.CreateDirectory(rootFolder);
                        }

                        string newFolderFullPath = rootFolder + newFolderName;

                        if (Directory.Exists(newFolderFullPath))
                        {
                            throw ExceptionHelper.GetModuleException("ID422008", null, null);
                        }
                        else
                        {
                            Directory.CreateDirectory(newFolderFullPath);
                        }
                    }
                    break;
                case "CommitTransaction":
                    {
                        //folder is already added in BeginTransaction
                        //everything is OK and nothing to do
                    }
                    break;
                case "RollBackTransaction":
                    {
                        //we should remove added folder
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFolderName = moduleService["Name"].ToString();
                        
                        string newFolderFullPath = rootFolder + newFolderName;

                        if (Directory.Exists(newFolderFullPath))
                        {
                            Directory.Delete(newFolderFullPath, true); 
                        }
                        else
                        {
                            //nothing to do
                        }
                    }
                    break;
            }

            

        }

        protected override void ExecuteRemove(Base.Module.ModuleService moduleService)
        {
            
            string deleteSufix = "_MarkedForDelete";
            switch (transContext)
            {
                case "BeginTransaction":
                    {
                        //we only rename folder to foldername_MarkedForDelete
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string folderName = moduleService["Name"].ToString();
                        string folderFullPath = rootFolder + folderName;

                        if (Directory.Exists(folderFullPath))
                        {
                            Directory.Move(folderFullPath, folderFullPath + deleteSufix);
                        }
                        else
                        {
                            throw ExceptionHelper.GetModuleException("ID422009", null, null);
                        }
                    }
                    break;
                case "CommitTransaction":
                    {
                        //we delete folder which have sufix deleteSufix
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string folderName = moduleService["Name"].ToString();
                        string folderFullPath = rootFolder + folderName;

                        if (Directory.Exists(folderFullPath + deleteSufix))
                        {
                            Directory.Delete(folderFullPath + deleteSufix, true);
                        }
                        else
                        {
                            //if folder dont exist nothing to do //throw ExceptionHelper.GetModuleException("ID422009", null, null);
                        }
                    }
                    break;
                case "RollBackTransaction":
                    {
                        //we should change name of folder folderFullPath + deleteSufix to folderFullPath
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string folderName = moduleService["Name"].ToString();
                        string folderFullPath = rootFolder + folderName;

                        if (Directory.Exists(folderFullPath + deleteSufix))
                        {
                            Directory.Move(folderFullPath + deleteSufix, folderFullPath);
                        }
                        else
                        {
                            //if folder don't exist nothing to do //throw ExceptionHelper.GetModuleException("ID422009", null, null);
                        }
                    }
                    break;
            }
            

        }

        protected override void ValidateService(Base.Module.ModuleService moduleService)
        {
            
        }

        public override string CallOperation(string operationName, string operationArgument)
        {
            string result = string.Empty;
            Atomia.Provisioning.Base.Exceptions.ModuleException exception = null;

            switch (operationName)
            {
                case "GetFullFolderPath":
                    {
                        result = resource["RootFolder"] + service["Name"];
                    }
                    break;
                default:
                    {
                        exception = ExceptionHelper.GetModuleException("ID422014", new Dictionary<string, string>() { { "Message", operationName } }, null);
                    }
                    break;
            }

            if (exception != null)
            {
                throw exception;
            }
            
            return result;

        }



    }
}
