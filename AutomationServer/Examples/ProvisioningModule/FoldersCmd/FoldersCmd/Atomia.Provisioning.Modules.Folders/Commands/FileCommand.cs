using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atomia.Provisioning.Base.Module;
using Atomia.Provisioning.Base;
using Atomia.Provisioning.Modules.Common;
using System.IO;

namespace Atomia.Provisioning.Modules.Folders.Commands
{
    public class FileCommand : ModuleCommandSimpleBase
    {

        private string transContext = string.Empty;

        public FileCommand(ModuleService service, ResourceDescription resource, ModuleService newServiceSettings, ModuleCommandType commandType, int listDepth, string transContext)
            : base(service, resource, newServiceSettings, commandType, listDepth)
        {
            this.transContext = transContext;
        }

        protected override void ExecuteModify(ModuleService oldService, ModuleService newService)
        {

            string changeFileSufix = "_CopyForRollBack";

            switch (transContext)
            {
                case "BeginTransaction":
                    {
                        //we copy old file to file with changeFileSufix sufix, and change file
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFileName = string.Empty;
                        string oldFileName = string.Empty;

                        newFileName = newService.Properties.Find(p => p.Name == "Name").Value;
                        oldFileName = oldService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = newService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string newFileFullPath = rootFolder + parentFolder + "\\" + newFileName;
                        string oldFileFullPath = rootFolder + parentFolder + "\\" + oldFileName;

                        string oldFileContent = oldService.Properties.Find(p => p.Name == "Content").Value;
                        string newFileContent = newService.Properties.Find(p => p.Name == "Content").Value;

                        File.Copy(oldFileFullPath, oldFileFullPath + changeFileSufix);

                        if (oldFileContent != newFileContent)
                        {
                            File.WriteAllText(oldFileFullPath, newFileContent);
                        }

                        if (oldFileName != newFileName)
                        {
                            File.Move(oldFileFullPath, newFileFullPath);
                            File.Delete(oldFileFullPath);
                        }
                        
                    }
                    break;
                case "CommitTransaction":
                    {
                        //change content of file, rename it to new name and delete copy with changeFileSufix
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFileName = string.Empty;
                        string oldFileName = string.Empty;

                        newFileName = newService.Properties.Find(p => p.Name == "Name").Value;
                        oldFileName = oldService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = newService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string newFileFullPath = rootFolder + parentFolder + "\\" + newFileName;
                        string oldFileFullPath = rootFolder + parentFolder + "\\" + oldFileName;
                        
                        File.Delete(oldFileFullPath + changeFileSufix);

                    }
                    break;
                case "RollBackTransaction":
                    {
                        //we should revert renamed file name, and file content
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string newFileName = string.Empty;
                        string oldFileName = string.Empty;

                        newFileName = newService.Properties.Find(p => p.Name == "Name").Value;
                        oldFileName = oldService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = newService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string newFileFullPath = rootFolder + parentFolder + "\\" + newFileName;
                        string oldFileFullPath = rootFolder + parentFolder + "\\" + oldFileName;

                        if (oldFileName == newFileName)
                        {
                            File.Delete(oldFileFullPath);
                        }
                        
                        File.Copy(oldFileFullPath + changeFileSufix, oldFileFullPath);

                    }
                    break;
            }


        }

        protected override void ExecuteAdd(Base.Module.ModuleService moduleService)
        {
            
            switch (transContext)
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

                        string fileName = moduleService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = moduleService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string fileFullPath = rootFolder + parentFolder + "\\" + fileName;

                        string fileContent = moduleService.Properties.Find(p => p.Name == "Content").Value;

                        if (File.Exists(fileFullPath))
                        {
                            throw ExceptionHelper.GetModuleException("ID422012", null, null);
                        }
                        else
                        {
                            File.WriteAllText(fileFullPath, fileContent);
                        }

                        bool bExist = moduleService.Properties.Exists(prop => prop.Name == "TestChangesPropagation");
                        if (bExist == false)
                        {
                            moduleService.Properties.Add(new ModuleServiceProperty("TestChangesPropagation", "NewValueForTesting"));
                        }
                        else
                        {
                            moduleService["TestChangesPropagation"] = "NewValueForTesting";
                        }

                    }
                    break;
                case "CommitTransaction":
                    {
                        //file is already added in BeginTransaction
                        //everything is OK and nothing to do
                    }
                    break;
                case "RollBackTransaction":
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

                        string fileName = moduleService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = moduleService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string fileFullPath = rootFolder + parentFolder + "\\" + fileName;

                        if (File.Exists(fileFullPath))
                        {
                            File.Delete(fileFullPath);
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
                        //we only rename file to filename_MarkedForDelete
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string fileName = moduleService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = moduleService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string fileFullPath = rootFolder + parentFolder + "\\" + fileName;

                        if (File.Exists(fileFullPath))
                        {
                            File.Move(fileFullPath, fileFullPath + deleteSufix);
                        }
                        else
                        {
                            throw ExceptionHelper.GetModuleException("ID422013", null, null);
                        }
                    }
                    break;
                case "CommitTransaction":
                    {
                        //we delete file which have sufix deleteSufix
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string fileName = moduleService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = moduleService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string fileFullPath = rootFolder + parentFolder + "\\" + fileName;

                        if (File.Exists(fileFullPath + deleteSufix))
                        {
                            File.Delete(fileFullPath + deleteSufix);
                        }
                        else
                        {
                            //if file dont exist nothing to do 
                        }
                    }
                    break;
                case "RollBackTransaction":
                    {
                        //we should change name of file fileFullPath + deleteSufix to fileFullPath
                        string rootFolder = this.resource["RootFolder"].ToString();
                        if (rootFolder == "")
                        {
                            rootFolder = "c:\\ProvisioningModuleTestFolder\\";
                        }

                        if (rootFolder.Substring(rootFolder.Length - 1, 1) != "\\")
                        {
                            rootFolder += "\\";
                        }

                        string fileName = moduleService.Properties.Find(p => p.Name == "Name").Value;

                        string parentFolder = moduleService.Properties.Find(p => p.Name == "ParentFolder").Value;

                        string fileFullPath = rootFolder + parentFolder + "\\" + fileName;

                        if (File.Exists(fileFullPath + deleteSufix))
                        {
                            File.Move(fileFullPath + deleteSufix, fileFullPath);
                        }
                        else
                        {
                            //if file don't exist nothing to do 
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
                case "GetFullFilePath":
                    {
                        result = resource["RootFolder"] + service["ParentFolder"] + "\\" + service["Name"];
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
