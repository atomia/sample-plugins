using System;
using System.Collections.Generic;
using Atomia.Provisioning.Modules.Common;
using Atomia.Provisioning.Base.Module;
using System.Reflection;
using System.IO;
using Atomia.Provisioning.Base;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
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

            switch (serviceName)
            {
                case "Folders":
                    {
                        command_type = typeof(Commands.FoldersCommand);
                    }
                    break;
                case "OtherCommand":
                    {
                        //command_type = typeof(Commands.OtherCommand);
                    }
                    break;
            }



            command = (ModuleCommand)Activator.CreateInstance(command_type, new object[] { childService[0], resource, commandType == ModuleCommandType.Modify ? childService[1] : null, commandType, listDepth });

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

        /// <summary>
        /// returns service description filename
        /// </summary>
        /// <returns></returns>
        private string GetServiceDescriptionAssemblyPath()
        {
            return "Atomia.Provisioning.Modules.Folders.ServiceDescription.xml";
        }
        

    }
}
