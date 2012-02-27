using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atomia.Provisioning.Base.Module;
using Atomia.Provisioning.Modules.Common;
using System.Reflection;
using System.IO;
using Atomia.Provisioning.Base;
using Atomia.Provisioning.Modules.Folders.Commands;
using System.Web.Script.Serialization;

namespace Atomia.Provisioning.Modules.Folders
{
    public class Folders : ModuleBase
    {

        /// List of commands in current transaction.
        /// </summary>
        private List<ModuleCommand> commands;

        /// <summary>
        /// Track whether Dispose has been called. 
        /// </summary>
        private bool disposed = false;

        string transactionContext = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Folders"/> class.
        /// </summary>
        public Folders()
        {
            this.commands = new List<ModuleCommand>();
        }

        #region ModuleBase Members

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public override void BeginTransaction()
        {
            if (this.commands.Count > 0)
            {
                throw ExceptionHelper.GetModuleException("ID400002", null, null);
            }
        }

        /// <summary>
        /// Calls the operation.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="operationArgument">The operation argument.</param>
        /// <param name="resource">The resource.</param>
        /// <returns>Operation result.</returns>
        public override string CallOperation(ModuleService service, string operationName, string operationArgument, Atomia.Provisioning.Base.ResourceDescription resource)
        {
            ModuleCommand command = this.GetModuleCommand(resource, ModuleCommandType.Add, -1, service);
            if (command is ModuleCommandSimpleBase)
            {
                return ((ModuleCommandSimpleBase)command).CallOperation(operationName, operationArgument);
            }
            else
            {
                throw ExceptionHelper.GetModuleException("ID400018", null, null);
            }
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public override void CommitTransaction()
        {
            foreach (ModuleCommand command in this.commands)
            {
                command.CleanUp();
            }

            this.commands.Clear();
        }

        /// <summary>
        /// Gets the module service description.
        /// </summary>
        /// <returns>Module service description.</returns>
        public override string GetModuleServiceDescription()
        {
            Assembly assembly = this.GetType().Assembly;
            Stream stream = assembly.GetManifestResourceStream(this.GetServiceDescriptionAssemblyPath());
            StreamReader reader = new StreamReader(stream);
            string description = reader.ReadToEnd();
            return description;
        }

        /// <summary>
        /// Lists the services.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="maxDepth">The max depth.</param>
        /// <param name="resource">The resource.</param>
        /// <returns>List of services.</returns>
        public override ModuleService ListServices(ModuleService service, int maxDepth, Atomia.Provisioning.Base.ResourceDescription resource)
        {
            ModuleCommand command = this.GetModuleCommand(resource, ModuleCommandType.List, maxDepth, service);
            command.Execute();
            return service;
        }

        /// <summary>
        /// Lists the services no children.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="resource">The resource.</param>
        /// <returns>List of services.</returns>
        public override List<ModuleService> ListServicesNoChildren(string serviceName, Atomia.Provisioning.Base.ResourceDescription resource)
        {
            throw ExceptionHelper.GetModuleException("ID400018", null, null);
        }

        /// <summary>
        /// Modifies the service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="newServiceSettings">The new service settings.</param>
        public override void ModifyService(ModuleService service, Atomia.Provisioning.Base.ResourceDescription resource, ModuleService newServiceSettings)
        {
            this.PrepareCommandList(service, newServiceSettings, resource, ModuleCommandType.Modify);
            this.PrepareAndExecuteCommandsFromList();
        }

        /// <summary>
        /// Moves to resource.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="currentResource">The current resource.</param>
        /// <param name="targetResource">The target resource.</param>
        public override void MoveToResource(ModuleService service, Atomia.Provisioning.Base.ResourceDescription currentResource, Atomia.Provisioning.Base.ResourceDescription targetResource)
        {
            throw ExceptionHelper.GetModuleException("ID400018", null, null);
        }

        /// <summary>
        /// Provides the service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="resource">The resource.</param>
        public override void ProvideService(ModuleService service, Atomia.Provisioning.Base.ResourceDescription resource)
        {
            this.PrepareCommandList(service, null, resource, ModuleCommandType.Add);
            this.PrepareAndExecuteCommandsFromList();
        }

        /// <summary>
        /// Removes the service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="resource">The resource.</param>
        public override void RemoveService(ModuleService service, Atomia.Provisioning.Base.ResourceDescription resource)
        {
            this.PrepareCommandList(service, null, resource, ModuleCommandType.Remove);
            this.PrepareAndExecuteCommandsFromList();
        }

        /// <summary>
        /// Rollbacks the transaction.
        /// </summary>
        public override void RollbackTransaction()
        {

            Exception rollback_expection = null;

            int size = this.commands.Count;
            for (int i = size - 1; i >= 0; i--)
            {
                try
                {
                    if (this.commands[i].Status == ModuleCommandStatus.Executed)
                    {
                        this.commands[i].Undo();
                    }

                    this.commands[i].CleanUp();
                }
                catch (Exception e)
                {
                    rollback_expection = e;
                }
            }

            this.commands.Clear();

            if (rollback_expection != null)
            {
                throw rollback_expection;
            }

        }

        #endregion ModuleBase Members
        
        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // dispose managed resources
                }

                // dispose unmanaged resources 
                this.disposed = true;
            }
        }

        #endregion IDisposable Members

        /// <summary>
        /// Gets the module command.
        /// </summary>
        /// <param name="resource">The server resource.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="childService">The child service.</param>
        /// <returns>Module command</returns>
        private ModuleCommand GetModuleCommand(ResourceDescription resource, ModuleCommandType commandType, int listDepth, params ModuleService[] childService)
        {

            ModuleCommand command = null;
            string serviceName = childService[0].Name;

            Type command_type = null;

            switch(serviceName)
            {
                case "Folders":
                    {
                        command_type = typeof(FolderCommand);
                    }
                    break;
                case "Files":
                    {
                        command_type = typeof(FileCommand);
                    }
                    break;
            }

            

            command = (ModuleCommand)Activator.CreateInstance(command_type, new object[] { childService[0], resource, commandType == ModuleCommandType.Modify ? childService[1] : null, commandType, listDepth, transactionContext });

            return command;

        }

        /// <summary>
        /// Prepares the command list.
        /// </summary>
        /// <param name="service">Service to populate.</param>
        /// <param name="newServiceSettings">New service settings.</param>
        /// <param name="resource">Server resource.</param>
        /// <param name="commandType">Type of command.</param>
        private void PrepareCommandList(ModuleService service, ModuleService newServiceSettings, ResourceDescription resource, ModuleCommandType commandType)
        {
            if (commandType != ModuleCommandType.Remove)
            {
                this.commands.Add(this.GetModuleCommand(resource, commandType, -1, service, newServiceSettings));
            }

            if (service.Children != null)
            {
                for (int i = 0; i < service.Children.Count; i++)
                {
                    ModuleService childService = service.Children[i];
                    ModuleService newChildSettings = null;
                    if (newServiceSettings != null && newServiceSettings.Children != null && newServiceSettings.Children.Count > i)
                    {
                        newChildSettings = newServiceSettings.Children[i];
                    }

                    this.PrepareCommandList(childService, newChildSettings, resource, commandType);
                }
            }

            if (commandType == ModuleCommandType.Remove)
            {
                this.commands.Add(this.GetModuleCommand(resource, commandType, -1, service, newServiceSettings));
            }
        }

        /// <summary>
        /// Prepares each commands from commands list and execute each commands from the same list. If error occurs during executing one command, Rollback is called.
        /// </summary>
        private void PrepareAndExecuteCommandsFromList()
        {
            foreach (ModuleCommand command in this.commands)
            {
                command.Prepare();
            }

            try
            {
                foreach (ModuleCommand command in this.commands)
                {
                    command.Execute();
                }
            }
            catch
            {
                this.RollbackTransaction();
                throw;
            }
        }

        private string GetServiceDescriptionAssemblyPath()
        {
            return "Atomia.Provisioning.Modules.Folders.ServiceDescription.xml";
        }

        /// <summary>
        /// Method which handle input arguments and call appropriate actions
        /// </summary>
        /// <param name="args"></param>
        public void HandleAndProvide(string[] args)
        {

            #region Test data as they are send from cmLocal or console

            //------------------------------------------------
            ////Add Test Args
            //"C:\Program Files (x86)\Atomia\AutomationServer\Common\Modules\Atomia.Provisioning.Modules.Folders.exe" Files BeginTransaction Add {"RootFolder":"c:\\TestProvisioningExample\\"} {"FileName":"TestFile.txt","ParentFolder":"ChangeMe","FileContent":"123"}
            //args = new string[5];
            //args[0] = "Files";
            //args[1] = "BeginTransaction";
            //args[2] = "Add";
            //args[3] = \"RootFolder\":\"c:\\TestProvisioningExample\\\";
            //args[4] = "{\"FileName\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"FileContent\":\"123\"}";

            //Modify Test Args
            //Files BeginTransaction Modify {"RootFolder":"c:\\TestProvisioningExample\\"} {"FileName":"TestFile.txt","ParentFolder":"ChangeMe","FileContent":"1234"} {"FileName":"TestFile.txt","ParentFolder":"ChangeMe","FileContent":"12345"}
            //args = new string[6];
            //args[0] = "Files";
            //args[1] = "BeginTransaction";
            //args[2] = "Modify";
            //args[3] = \"RootFolder\":\"c:\\TestProvisioningExample\\\";
            //args[4] = "{\"FileName\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"FileContent\":\"123\"}";
            //args[5] = "{\"FileName\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"FileContent\":\"12345sdfsdf\"}";

            //Add Test Args
            //"C:\Program Files (x86)\Atomia\AutomationServer\Common\Modules\Atomia.Provisioning.Modules.Folders.exe" Folders BeginTransaction Add {"RootFolder":"c:\\TestProvisioningExample\\"} {"FolderName":"ChangeMe454","FolderDescription":"Change this default description value"}

            //Folders 
            //BeginTransaction 
            //Add 
            //{"RootFolder":"c:\\TestProvisioningExample\\"} 
            //{"FolderName":"ChangeMe","FolderDescription":"Change this default description value"} 
            //{"current":{"serviceMetaData_InstanceId":"65782c7f-8072-4b2e-a8c8-1d116e6988a7","serviceMetaData_LogicalID":"","serviceMetaData_Name":"Folders","serviceMetaData_PhysicalID":"65782c7f-8072-4b2e-a8c8-1d116e6988a7","serviceMetaData_ProvisioningId":"65782c7f-8072-4b2e-a8c8-1d116e6988a7","AddCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--Add_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--FolderName_#_#_{service[Name]}_#_#_--FolderDescription_#_#_{service[Description]}","CallOpCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--CallOp","Description":"Change_#_#_this_#_#_default_#_#_description_#_#_value","foo":"222","GetServiceDescCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--GetServiceDescCmd","Id":"123","ListChildrenCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListChildren","ListCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--List","ListNoChildrenCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListNoChildren","ModCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--Modify_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--FolderName_#_#_{service[Name]}_#_#_--FolderDescription_#_#_{service[Description]}","MoveToResCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--MoveToResource","Name":"ChangeMe","RemCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--Remove_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--FolderName_#_#_{service[Name]}_#_#_--FolderDescription_#_#_{service[Description]}","StdinStdout":"true","SynchExpPropCmd":"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--SynchExpProp"}}


            //args = new string[6];
            //args[0] = "Folders";
            //args[1] = "BeginTransaction";
            //args[2] = "Add";
            //args[3] = \"RootFolder\":\"c:\\TestProvisioningExample\\\";
            //args[4] = "{\"Name\":\"ChangeMe\",\"Description\":\"Change this default description value\"}";
            //args[5] = "{\"current\":{\"serviceMetaData_InstanceId\":\"65782c7f-8072-4b2e-a8c8-1d116e6988a7\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"65782c7f-8072-4b2e-a8c8-1d116e6988a7\",\"serviceMetaData_ProvisioningId\":\"65782c7f-8072-4b2e-a8c8-1d116e6988a7\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Add_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--FolderName_#_#_{service[Name]}_#_#_--FolderDescription_#_#_{service[Description]}\",\"CallOpCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--CallOp\",\"Description\":\"Change_#_#_this_#_#_default_#_#_description_#_#_value\",\"foo\":\"222\",\"GetServiceDescCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--GetServiceDescCmd\",\"Id\":\"123\",\"ListChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListChildren\",\"ListCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--List\",\"ListNoChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListNoChildren\",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Modify_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--FolderName_#_#_{service[Name]}_#_#_--FolderDescription_#_#_{service[Description]}\",\"MoveToResCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--MoveToResource\",\"Name\":\"ChangeMe\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Remove_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--FolderName_#_#_{service[Name]}_#_#_--FolderDescription_#_#_{service[Description]}\",\"StdinStdout\":\"true\",\"SynchExpPropCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--SynchExpProp\"}}";

            //Folders BeginTransaction Modify {"RootFolder":"c:\\TestProvisioningExample\\"} {"FolderName":"ChangeMe","FolderDescription":"Change this default description value"} {"FolderName":"ChangeMeddd","FolderDescription":"Change this default description value"}
            //args = new string[6];
            //args[0] = "Folders";
            //args[1] = "BeginTransaction";
            //args[2] = "Modify";
            //args[3] = "\"RootFolder\":\"c:\\TestProvisioningExample\\\"";
            //args[4] = "{\"FolderName\":\"ChangeMe\",\"FolderDescription\":\"Change this default description value\"}";
            //args[5] = "{\"FolderName\":\"ChangeMeddd\",\"FolderDescription\":\"Change this default description value\"}";


            //Files CommitTransaction Add {"RootFolder":"c:\\TestProvisioningExample\\"} 
            //{"Name":"TestFile.txt","ParentFolder":"ChangeMe","Content":"123"} 
            //{"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\"","\"serviceMetaData_ProvisioningId\"":"\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\"","\"AddCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Add_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--ParentFolder_#_#_{service[parent.Name]}_#_#_--Content_#_#_{service[Content]}\"","\"Content\"":"\"123\"","\"Id\"":"\"123\"","\"ModCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Modify_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--ParentFolder_#_#_{service[parent.Name]}_#_#_--Content_#_#_{service[Content]}\"","\"Name\"":"\"TestFile.txt\"","\"ParentFolder\"":"\"WillTakeValueFromParentService\"","\"RemCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Remove_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--ParentFolder_#_#_{service[parent.Name]}_#_#_--Content_#_#_{service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\"","\"serviceMetaData_LogicalID\"":"\"88788e53-6292-43a7-959c-9a4158792ec1\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"88788e53-6292-43a7-959c-9a4158792ec1\"","\"serviceMetaData_ProvisioningId\"":"\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\"","\"AddCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Add_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--Description_#_#_{service[Description]}\"","\"CallOpCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--CallOp\"","\"Description\"":"\"Change_#_#_this_#_#_default_#_#_description_#_#_value\"","\"foo\"":"\"222\"","\"GetServiceDescCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--GetServiceDescCmd\"","\"Id\"":"\"123\"","\"ListChildrenCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListChildren\"","\"ListCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--List\"","\"ListNoChildrenCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListNoChildren\"","\"ModCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Modify_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--Description_#_#_{service[Description]}\"","\"MoveToResCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--MoveToResource\"","\"Name\"":"\"ChangeMe\"","\"RemCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Remove_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--Description_#_#_{service[Description]}\"","\"StdinStdout\"":"\"true\"","\"SynchExpPropCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--SynchExpProp\""}}
            //args = new string[6];
            //args[0] = "Files";
            //args[1] = "CommitTransaction";
            //args[2] = "Add";
            //args[3] = "\"RootFolder\":\"c:\\TestProvisioningExample\\\"";
            //args[4] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"123\"}";
            //args[5] = "{\"current\":{\"serviceMetaData_InstanceId\":\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Files\",\"serviceMetaData_PhysicalID\":\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\"" +
            //    ",\"serviceMetaData_ProvisioningId\":\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Add_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--ParentFolder_#_#_{service[parent.Name]}_#_#_--Content_#_#_{service[Content]}\"" +
            //    ",\"Content\":\"123\",\"Id\":\"123\",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Modify_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--ParentFolder_#_#_{service[parent.Name]}_#_#_--Content_#_#_{service[Content]}\"" +
            //    ",\"Name\":\"TestFile.txt\",\"ParentFolder\":\"WillTakeValueFromParentService\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Remove_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--ParentFolder_#_#_{service[parent.Name]}_#_#_--Content_#_#_{service[Content]}\",\"StdinStdout\":\"true\",\"TestChangesPropagation\":\"\"},\"current.parent\":{\"serviceMetaData_InstanceId\":\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\",\"serviceMetaData_LogicalID\":\"88788e53-6292-43a7-959c-9a4158792ec1\",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"88788e53-6292-43a7-959c-9a4158792ec1\",\"serviceMetaData_ProvisioningId\":\"dd41fa6e-1524-49dd-a9b3-c515a230edf4\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Add_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--Description_#_#_{service[Description]}\",\"CallOpCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--CallOp\",\"Description\":\"Change_#_#_this_#_#_default_#_#_description_#_#_value\",\"foo\":\"222\",\"GetServiceDescCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--GetServiceDescCmd\",\"Id\":\"123\",\"ListChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListChildren\",\"ListCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--List\",\"ListNoChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--ListNoChildren\",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Modify_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--Description_#_#_{service[Description]}\",\"MoveToResCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--MoveToResource\",\"Name\":\"ChangeMe\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--Remove_#_#_--RootFolder_#_#_{resource[RootFolder]}_#_#_--Name_#_#_{service[Name]}_#_#_--Description_#_#_{service[Description]}\",\"StdinStdout\":\"true\",\"SynchExpPropCmd\":\"Atomia.Provisioning.Modules.Folders.exe_#_#_--$TransGuid_#_#_--SynchExpProp\"}}";


            //Files 
            //BeginTransaction 
            //Modify 
            //{"RootFolder":"c:\\TestProvisioningExample\\"} 
            //{"Name":"TestFile.txt","ParentFolder":"ChangeMe","Content":"12355566rere"} 
            //{"Name":"TestFile.txt","ParentFolder":"ChangeMe","Content":"12355566rereee"} 
            //{"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"serviceMetaData_ProvisioningId\"":"\"\"","\"AddCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"12355566rere\"","\"Id\"":"\"123\"","\"ModCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile.txt\"","\"ParentFolder\"":"\"WillTakeValueFromParentService\"","\"RemCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"false\"","\"TestChangesPropagation\"":"\"\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"serviceMetaData_LogicalID\"":"\"d287c05f-47eb-4699-ad9e-2900dcd0f570\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"d287c05f-47eb-4699-ad9e-2900dcd0f570\"","\"serviceMetaData_ProvisioningId\"":"\"\"","\"AddCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"CallOpCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --CallOp\"","\"Description\"":"\"Change this default description value\"","\"foo\"":"\"222\"","\"GetServiceDescCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --GetServiceDescCmd\"","\"Id\"":"\"123\"","\"ListChildrenCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListChildren\"","\"ListCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --List\"","\"ListNoChildrenCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListNoChildren\"","\"ModCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"MoveToResCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --MoveToResource\"","\"Name\"":"\"ChangeMe\"","\"RemCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"SynchExpPropCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --SynchExpProp\""}} 
            //{"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"serviceMetaData_ProvisioningId\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"AddCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"12355566rereee\"","\"Id\"":"\"123\"","\"ModCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile.txt\"","\"ParentFolder\"":"\"WillTakeValueFromParentService\"","\"RemCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"serviceMetaData_LogicalID\"":"\"d287c05f-47eb-4699-ad9e-2900dcd0f570\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"d287c05f-47eb-4699-ad9e-2900dcd0f570\"","\"serviceMetaData_ProvisioningId\"":"\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"","\"AddCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"CallOpCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --CallOp\"","\"Description\"":"\"Change this default description value\"","\"foo\"":"\"222\"","\"GetServiceDescCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --GetServiceDescCmd\"","\"Id\"":"\"123\"","\"ListChildrenCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListChildren\"","\"ListCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --List\"","\"ListNoChildrenCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListNoChildren\"","\"ModCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"MoveToResCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --MoveToResource\"","\"Name\"":"\"ChangeMe\"","\"RemCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"SynchExpPropCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --SynchExpProp\""}}
            //args = new string[8];
            //args[0] = "Files";
            //args[1] = "BeginTransaction";
            //args[2] = "Modify";
            //args[3] = "\"RootFolder\":\"c:\\TestProvisioningExample\\\"";
            //args[4] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"12355566rere\"}";
            //args[5] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"12355566rereee\"}";
            //args[6] = "{\"current\":{\"serviceMetaData_InstanceId\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Files\",\"serviceMetaData_PhysicalID\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\"" +
            //",\"serviceMetaData_ProvisioningId\":\"\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"" + 
            //",\"Content\":\"12355566rere\",\"Id\":\"123\",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"" + 
            //",\"Name\":\"TestFile.txt\",\"ParentFolder\":\"WillTakeValueFromParentService\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"" + 
            //",\"StdinStdout\":\"false\",\"TestChangesPropagation\":\"\"},\"current.parent\":{\"serviceMetaData_InstanceId\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"serviceMetaData_LogicalID\":\"d287c05f-47eb-4699-ad9e-2900dcd0f570\"" + 
            //",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"d287c05f-47eb-4699-ad9e-2900dcd0f570\",\"serviceMetaData_ProvisioningId\":\"\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"" + 
            //",\"CallOpCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --CallOp\",\"Description\":\"Change this default description value\",\"foo\":\"222\",\"GetServiceDescCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --GetServiceDescCmd\",\"Id\":\"123\"" + 
            //",\"ListChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListChildren\",\"ListCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --List\",\"ListNoChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListNoChildren\"" + 
            //",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"MoveToResCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --MoveToResource\",\"Name\":\"ChangeMe\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"StdinStdout\":\"false\",\"SynchExpPropCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --SynchExpProp\"}}";
            //args[7] = "{\"current\":{\"serviceMetaData_InstanceId\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Files\",\"serviceMetaData_PhysicalID\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"serviceMetaData_ProvisioningId\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Content\":\"12355566rereee\",\"Id\":\"123\",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Name\":\"TestFile.txt\",\"ParentFolder\":\"WillTakeValueFromParentService\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"StdinStdout\":\"true\",\"TestChangesPropagation\":\"\"},\"current.parent\":{\"serviceMetaData_InstanceId\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"serviceMetaData_LogicalID\":\"d287c05f-47eb-4699-ad9e-2900dcd0f570\",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"d287c05f-47eb-4699-ad9e-2900dcd0f570\",\"serviceMetaData_ProvisioningId\":\"a47a2f24-c64e-4585-97a4-7ba052d3be4f\",\"AddCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"CallOpCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --CallOp\",\"Description\":\"Change this default description value\",\"foo\":\"222\",\"GetServiceDescCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --GetServiceDescCmd\",\"Id\":\"123\",\"ListChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListChildren\",\"ListCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --List\",\"ListNoChildrenCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --ListNoChildren\",\"ModCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"MoveToResCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --MoveToResource\",\"Name\":\"ChangeMe\",\"RemCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"StdinStdout\":\"false\",\"SynchExpPropCmd\":\"Atomia.Provisioning.Modules.Folders.exe --$TransGuid --SynchExpProp\"}}";

            //Files BeginTransaction Add {"RootFolder":"c:\\TestProvisioningExample\\"} {"FileName":"TestFile.txt","ParentFolder":"ChangeMe","FileContent":"123"}

            //args = new string[5];
            //args[0] = "Files";
            //args[1] = "BeginTransaction";
            //args[2] = "Add";
            //args[3] = "{\"RootFolder\":\"c:\\TestProvisioningExample\\\"}";
            //args[4] = "{\"FileName\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"FileContent\":\"123\"}";


            ////Files BeginTransaction Add {"RootFolder":"c:\\TestProvisioningExample\\"} {"Name":"TestFile.txt","ParentFolder":"ChangeMe","Content":"123"} {"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\"","\"serviceMetaData_ProvisioningId\"":"\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"123\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile.txt\"","\"OperationMapping\"":"\"GetFullFilePath:GetFullFilePath\"","\"ParentFolder\"":"\"WillTakeValueFromParentService\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"\"","\"UseCmdLinePlugin\"":"\"true\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\"","\"serviceMetaData_LogicalID\"":"\"11aed34b-0c44-4f68-bc61-02c18ebc7012\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"11aed34b-0c44-4f68-bc61-02c18ebc7012\"","\"serviceMetaData_ProvisioningId\"":"\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Description\"":"\"Change this default description value\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Name\"":"\"ChangeMe\"","\"OperationMapping\"":"\"GetFullFolderPath:GetFullFolderPath\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"UseCmdLinePlugin\"":"\"true\""}}
            //args = new string[6];
            //args[0] = "Files";
            //args[1] = "BeginTransaction";
            //args[2] = "Add";
            //args[3] = "{\"RootFolder\":\"c:\\TestProvisioningExample\\\"}";
            //args[4] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"123\"}";
            //args[5] = "{\"current\":{\"serviceMetaData_InstanceId\":\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Files\",\"serviceMetaData_PhysicalID\":\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\",\"serviceMetaData_ProvisioningId\":\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\",\"AddCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"AddExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"AddRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Content\":\"123\",\"Id\":\"123\",\"ModifyCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"ModifyExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"ModifyRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Name\":\"TestFile.txt\",\"OperationMapping\":\"GetFullFilePath:GetFullFilePath\",\"ParentFolder\":\"WillTakeValueFromParentService\",\"RemoveCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"RemoveExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"RemoveRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"StdinStdout\":\"true\",\"TestChangesPropagation\":\"\",\"UseCmdLinePlugin\":\"true\"},\"current.parent\":{\"serviceMetaData_InstanceId\":\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\",\"serviceMetaData_LogicalID\":\"11aed34b-0c44-4f68-bc61-02c18ebc7012\",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"11aed34b-0c44-4f68-bc61-02c18ebc7012\",\"serviceMetaData_ProvisioningId\":\"321220b0-f0ca-46b7-9b70-6a0a88c19d7e\",\"AddCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"AddExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"AddRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"Description\":\"Change this default description value\",\"Id\":\"123\",\"ModifyCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"ModifyExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"ModifyRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"Name\":\"ChangeMe\",\"OperationMapping\":\"GetFullFolderPath:GetFullFolderPath\",\"RemoveCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"RemoveExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"RemoveRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"StdinStdout\":\"false\",\"UseCmdLinePlugin\":\"true\"}}";


            //Files BeginTransaction Modify {"RootFolder":"c:\\TestProvisioningExample\\"} {"Name":"TestFile.txt","ParentFolder":"ChangeMe","Content":"123"} {"Name":"TestFile2.txt","ParentFolder":"ChangeMe","Content":"123"} {"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"serviceMetaData_ProvisioningId\"":"\"\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"123\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {s\r\nervice[parent.Name]} --Content {service[Content]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile.txt\"","\"OperationMapping\"":"\"GetFullFilePath:GetFullFilePath\"","\"ParentFolder\"":"\"ChangeMe\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {res\r\nource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"NewValueForTesting\"","\"UseCmdLinePlugin\"":"\"true\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"serviceMetaData_LogicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_ProvisioningId\"":"\"\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Description\"":"\"Change this default description value\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Name\"":"\"ChangeMe\"","\"OperationMapping\"":"\"GetFullFolderPath:GetFullFolderPath\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"UseCmdLinePlugin\"":"\"true\""}} {"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"serviceMetaData_ProvisioningId\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"123\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {s\r\nervice[parent.Name]} --Content {service[Content]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile2.txt\"","\"OperationMapping\"":"\"GetFullFilePath:GetFullFilePath\"","\"ParentFolder\"":"\"ChangeMe\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {res\r\nource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"NewValueForTesting\"","\"UseCmdLinePlugin\"":"\"true\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"serviceMetaData_LogicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_ProvisioningId\"":"\"9c79cf42-d623-4e84-abab-449dfa505a6f\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Description\"":"\"Change this default description value\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Name\"":"\"ChangeMe\"","\"OperationMapping\"":"\"GetFullFolderPath:GetFullFolderPath\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"UseCmdLinePlugin\"":"\"true\""}}
            //args = new string[8];
            //args[0] = "Files";
            //args[1] = "BeginTransaction";
            //args[2] = "Modify";
            //args[3] = "{\"RootFolder\":\"c:\\TestProvisioningExample\\\"}";
            //args[4] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"123\"}";
            //args[5] = "{\"Name\":\"TestFile2.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"123\"}";
            //args[6] = "{\"current\":{\"serviceMetaData_InstanceId\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Files\",\"serviceMetaData_PhysicalID\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"serviceMetaData_ProvisioningId\":\"\",\"AddCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"AddExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"AddRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Content\":\"123\",\"Id\":\"123\",\"ModifyCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {s\r\nervice[parent.Name]} --Content {service[Content]}\",\"ModifyExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"ModifyRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Name\":\"TestFile.txt\",\"OperationMapping\":\"GetFullFilePath:GetFullFilePath\",\"ParentFolder\":\"ChangeMe\",\"RemoveCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"RemoveExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"RemoveRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {res\r\nource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"StdinStdout\":\"true\",\"TestChangesPropagation\":\"NewValueForTesting\",\"UseCmdLinePlugin\":\"true\"},\"current.parent\":{\"serviceMetaData_InstanceId\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"serviceMetaData_LogicalID\":\"6663a64d-bf34-4b64-ba3d-ef818e114572\",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"6663a64d-bf34-4b64-ba3d-ef818e114572\",\"serviceMetaData_ProvisioningId\":\"\",\"AddCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"AddExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"AddRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"Description\":\"Change this default description value\",\"Id\":\"123\",\"ModifyCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"ModifyExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"ModifyRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"Name\":\"ChangeMe\",\"OperationMapping\":\"GetFullFolderPath:GetFullFolderPath\",\"RemoveCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"RemoveExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"RemoveRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"StdinStdout\":\"false\",\"UseCmdLinePlugin\":\"true\"}}"; 
            //args[7] = "{\"current\":{\"serviceMetaData_InstanceId\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"serviceMetaData_LogicalID\":\"\",\"serviceMetaData_Name\":\"Files\",\"serviceMetaData_PhysicalID\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"serviceMetaData_ProvisioningId\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"AddCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"AddExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"AddRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Content\":\"123\",\"Id\":\"123\",\"ModifyCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {s\r\nervice[parent.Name]} --Content {service[Content]}\",\"ModifyExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"ModifyRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"Name\":\"TestFile2.txt\",\"OperationMapping\":\"GetFullFilePath:GetFullFilePath\",\"ParentFolder\":\"ChangeMe\",\"RemoveCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"RemoveExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"RemoveRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {res\r\nource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\",\"StdinStdout\":\"true\",\"TestChangesPropagation\":\"NewValueForTesting\",\"UseCmdLinePlugin\":\"true\"},\"current.parent\":{\"serviceMetaData_InstanceId\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"serviceMetaData_LogicalID\":\"6663a64d-bf34-4b64-ba3d-ef818e114572\",\"serviceMetaData_Name\":\"Folders\",\"serviceMetaData_PhysicalID\":\"6663a64d-bf34-4b64-ba3d-ef818e114572\",\"serviceMetaData_ProvisioningId\":\"9c79cf42-d623-4e84-abab-449dfa505a6f\",\"AddCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"AddExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"AddRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"Description\":\"Change this default description value\",\"Id\":\"123\",\"ModifyCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"ModifyExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"ModifyRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"Name\":\"ChangeMe\",\"OperationMapping\":\"GetFullFolderPath:GetFullFolderPath\",\"RemoveCommitCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"RemoveExecuteCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"RemoveRollbackCmd\":\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\",\"StdinStdout\":\"false\",\"UseCmdLinePlugin\":\"true\"}}";


            //Files CommitTransaction Modify {"RootFolder":"c:\\TestProvisioningExample\\"} {"Name":"TestFile.txt","Content":"123"} {"Name":"TestFile2.txt","Content":"123"} {"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"serviceMetaData_ProvisioningId\"":"\"\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"123\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {s\r\nervice[parent.Name]} --Content {service[Content]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile.txt\"","\"OperationMapping\"":"\"GetFullFilePath:GetFullFilePath\"","\"ParentFolder\"":"\"ChangeMe\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {res\r\nource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"NewValueForTesting\"","\"UseCmdLinePlugin\"":"\"true\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"serviceMetaData_LogicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_ProvisioningId\"":"\"\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Description\"":"\"Change this default description value\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Name\"":"\"ChangeMe\"","\"OperationMapping\"":"\"GetFullFolderPath:GetFullFolderPath\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"UseCmdLinePlugin\"":"\"true\""}} {"\"current\"":{"\"serviceMetaData_InstanceId\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"serviceMetaData_LogicalID\"":"\"\"","\"serviceMetaData_Name\"":"\"Files\"","\"serviceMetaData_PhysicalID\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"serviceMetaData_ProvisioningId\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Content\"":"\"123\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {s\r\n\r\nervice[parent.Name]} --Content {service[Content]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"Name\"":"\"TestFile2.txt\"","\"OperationMapping\"":"\"GetFullFilePath:GetFullFilePath\"","\"ParentFolder\"":"\"ChangeMe\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder\r\n {res\r\nource[RootFolder]} --Name {service[Name]} --ParentFolder {service[parent.Name]} --Content {service[Content]}\"","\"StdinStdout\"":"\"true\"","\"TestChangesPropagation\"":"\"NewValueForTesting\"","\"UseCmdLinePlugin\"":"\"true\""},"\"current.parent\"":{"\"serviceMetaData_InstanceId\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"serviceMetaData_LogicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_Name\"":"\"Folders\"","\"serviceMetaData_PhysicalID\"":"\"6663a64d-bf34-4b64-ba3d-ef818e114572\"","\"serviceMetaData_ProvisioningId\"":"\"89e49fe1-d5ef-42ca-9b7c-986fb841ca96\"","\"AddCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"AddRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Add --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Description\"":"\"Change this default description value\"","\"Id\"":"\"123\"","\"ModifyCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"ModifyRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Modify --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"Name\"":"\"ChangeMe\"","\"OperationMapping\"":"\"GetFullFolderPath:GetFullFolderPath\"","\"RemoveCommitCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveExecuteCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"RemoveRollbackCmd\"":"\"Atomia.Provisioning.Modules.Folders.exe --Remove --RootFolder {resource[RootFolder]} --Name {service[Name]} --Description {service[Description]}\"","\"StdinStdout\"":"\"false\"","\"UseCmdLinePlugin\"":"\"true\""}}
            //args = new string[8];
            //args[0] = "Files";
            //args[1] = "CommitTransaction";
            //args[2] = "Modify";
            //args[3] = "{\"RootFolder\":\"c:\\TestProvisioningExample\\\"}";
            //args[4] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"123\"}";
            //args[5] = "{\"Name\":\"TestFile2.txt\",\"ParentFolder\":\"ChangeMe\",\"Content\":\"123\"}";
            //args[6] = "";
            //args[7] = "";
            
            //Folders CallOperation operationName:GetFullFolderPath operationArgument: {"RootFolder":"c:\\TestProvisioningExample\\"} {"Name":"ChangeMe"}
            //args = new string[6];
            //args[0] = "Folders";
            //args[1] = "CallOperation";
            //args[2] = "operationName:GetFullFolderPath";
            //args[3] = "operationArguments:";
            //args[4] = "{\"RootFolder\":\"c:\\TestProvisioningExample\\\"}";
            //args[5] = "{\"Name\":\"ChangeMe\"}";

            //Files CallOperation operationName:GetFullFilePath operationArgument: {"RootFolder":"c:\\TestProvisioningExample\\"} {"Name":"TestFile.txt","ParentFolder":"WillTakeValueFromParentService"}
            //args = new string[6];
            //args[0] = "Files";
            //args[1] = "CallOperation";
            //args[2] = "operationName:GetFullFilePath";
            //args[3] = "operationArguments:";
            //args[4] = "{\"RootFolder\":\"c:\\TestProvisioningExample\\\"}";
            //args[5] = "{\"Name\":\"TestFile.txt\",\"ParentFolder\":\"WillTakeValueFromParentService\"}";

            #endregion Test data as they are send from console

            #region Check args and prepare variables
            //serviceName TransContext CommandType Resource [Service] [ServiceOld ServiceNew for modify command] [full services serialized]
            //Folders CommitTransaction Add {"RootFolder":"c:\\TestProvisioningExample\\"} {"FolderName":"ChangeMe","FolderDescription":"Change this default description value"}

            if (args == null)
            {
                throw ExceptionHelper.GetModuleException("ID422001", null, null);
            }
            if (args.Length < 3)
            {
                throw ExceptionHelper.GetModuleException("ID422001", null, null);
            }

            string serviceName = args[0].ToString();
            string commandType = string.Empty;

            switch (args[1])
            {
                case "CallOperation":
                    {
                        commandType = args[1].ToString();
                    }
                    break;
                default:
                    {
                        transactionContext = args[1].ToString().Replace("$", "");
                        commandType = args[2].ToString();
                    }
                    break;
            }

            ModuleService service = new ModuleService(serviceName, "");
            ModuleService serviceOld = new ModuleService(serviceName, "");
            ModuleService serviceNew = new ModuleService(serviceName, "");

            ModuleService serviceFull = null;
            ModuleService serviceOldFull = null;
            ModuleService serviceNewFull = null;

            ResourceDescription resource = new ResourceDescription(serviceName, "ResourceDescriptionFile");

            #endregion Check args and prepare variables

            switch (commandType)
            {
                case "Add":
                case "Remove":
                    {
                        #region Add/Remove Handling

                        #region Check args number
                        if (!((args.Length == 5) || (args.Length == 6)))
                        {
                            throw ExceptionHelper.GetModuleException("ID422002", null, null);
                        }
                        #endregion Check args number

                        resource = Helpers.Common.ReadAndFillResourceData(args[3], serviceName);

                        service = Helpers.Common.ReadAndFillServiceData(args[4], serviceName);

                        #region Read and fill full serialized service
                        if (args.Length == 6)
                        {
                            serviceFull = Helpers.Common.DeserializeService(args[5]);
                        }
                        #endregion Read and fill full serialized service

                        switch (commandType)
                        {
                            case "Add":
                                {
                                    ProvideService(service, resource);
                                }
                                break;

                            case "Remove":
                                {
                                    RemoveService(service, resource);
                                }
                                break;
                        }

                        #region After add we should update service properties if any is changed, if StdinStdOut is true
                        if (serviceFull != null)
                        {
                            if (serviceFull["StdinStdout"].ToLower() == "true")
                            {
                                foreach (ModuleServiceProperty prop in service.Properties)
                                {
                                    serviceFull[prop.Name] = prop.Value;
                                    //serviceFull.Properties.Add(new ModuleServiceProperty(prop.Name, prop.Value));
                                }
                                string jsonSerializedFullService = Helpers.Common.SerializeService(serviceFull);
                                
                                bool bLoop = true;
                                string sCurrentMessagePart = string.Empty;
                                string sRestOfMessage = jsonSerializedFullService;
                                while (bLoop)
                                {
                                    if (sRestOfMessage.Length < 1024)
                                    {
                                        sCurrentMessagePart = sRestOfMessage;
                                        bLoop = false;
                                    }
                                    else
                                    {
                                        sCurrentMessagePart = sRestOfMessage.Substring(0, 1023);
                                        sRestOfMessage = sRestOfMessage.Replace(sCurrentMessagePart, "");
                                    }

                                    Console.WriteLine(sCurrentMessagePart);

                                }
                                
                            }
                        }
                        #endregion After add we should update service properties if any is changed, if StdinStdOut is true
                        
                        #endregion Add/Remove Handling
                    }
                    break;

                case "Modify":
                    {
                        #region Modify Handling

                        #region Check args number
                        if (!((args.Length == 6) || (args.Length == 8)))
                        {
                            throw ExceptionHelper.GetModuleException("ID422003", null, null);
                        }
                        #endregion Check args number

                        resource = Helpers.Common.ReadAndFillResourceData(args[3], serviceName);

                        serviceOld = Helpers.Common.ReadAndFillServiceData(args[4], serviceName);

                        serviceNew = Helpers.Common.ReadAndFillServiceData(args[5], serviceName);

                        #region Read and fill full serialized service
                        if (args.Length == 8)
                        {
                            serviceOldFull = Helpers.Common.DeserializeService(args[6]);
                            serviceNewFull = Helpers.Common.DeserializeService(args[7]);
                        }
                        #endregion Read and fill full serialized service

                        ModifyService(serviceOld, resource, serviceNew);

                        #region After modify we should update service properties if any is changed, if StdinStdOut is true
                        if (serviceNewFull != null)
                        {
                            if (serviceNewFull["StdinStdout"].ToLower() == "true")
                            {
                                foreach (ModuleServiceProperty prop in serviceNew.Properties)
                                {
                                    serviceNewFull[prop.Name] = prop.Value;
                                }
                                string jsonSerializedFullService = Helpers.Common.SerializeService(serviceNewFull);

                                bool bLoop = true;
                                string sCurrentMessagePart = string.Empty;
                                string sRestOfMessage = jsonSerializedFullService;
                                while (bLoop)
                                {
                                    if (sRestOfMessage.Length < 1024)
                                    {
                                        sCurrentMessagePart = sRestOfMessage;
                                        bLoop = false;
                                    }
                                    else
                                    {
                                        sCurrentMessagePart = sRestOfMessage.Substring(0, 1023);
                                        sRestOfMessage = sRestOfMessage.Replace(sCurrentMessagePart, "");
                                    }

                                    Console.WriteLine(sCurrentMessagePart);
                                }
                            }
                        }
                        #endregion after modify we should update service properties if any is changed, if StdinStdOut is true
                        
                        #endregion Modify Handling
                    }
                    break;

                case "CallOperation":
                    {
                        #region CallOperation Handling

                        #region Check args number
                        if (args.Length != 6)
                        {
                            throw ExceptionHelper.GetModuleException("ID422002", null, null);
                        }
                        #endregion Check args number

                        //string args = service.Name + " " + ModuleCommandType.CallOperation.ToString() + " " + "operationName:" + operationName + " " + "operationArgument:" + operationArgument + " " + jsonSerializedResourceProperties + " " + jsonSerializedServiceProperties;

                        string operationName = string.Empty;
                        string operationArgument = string.Empty;

                        string oneParam = args[2];
                        string[] oneParamSplitted = oneParam.Split(":".ToArray(), 2, StringSplitOptions.None);
                        if (oneParamSplitted.Length != 2)
                        {
                            throw ExceptionHelper.GetModuleException("ID422001", null, null);
                        }
                        else
                        {
                            operationName = oneParamSplitted[1];
                        }

                        oneParam = args[3];
                        oneParamSplitted = oneParam.Split(":".ToArray(), 2, StringSplitOptions.None);
                        if (oneParamSplitted.Length != 2)
                        {
                            throw ExceptionHelper.GetModuleException("ID422001", null, null);
                        }
                        else
                        {
                            operationArgument = oneParamSplitted[1];
                        }

                        resource = Helpers.Common.ReadAndFillResourceData(args[4], serviceName);
                        service = Helpers.Common.ReadAndFillServiceData(args[5], serviceName);

                        string operationResult = CallOperation(service, operationName, operationArgument, resource);
                        
                        bool bLoop = true;
                        string sCurrentMessagePart = string.Empty;
                        string sRestOfMessage = operationResult;
                        while (bLoop)
                        {
                            if (sRestOfMessage.Length < 1024)
                            {
                                sCurrentMessagePart = sRestOfMessage;
                                bLoop = false;
                            }
                            else
                            {
                                sCurrentMessagePart = sRestOfMessage.Substring(0, 1023);
                                sRestOfMessage = sRestOfMessage.Replace(sCurrentMessagePart, "");
                            }

                            Console.WriteLine(sCurrentMessagePart);
                        }

                        #endregion CallOperation Handling
                    }
                    break;
            }

        }

    }

}
