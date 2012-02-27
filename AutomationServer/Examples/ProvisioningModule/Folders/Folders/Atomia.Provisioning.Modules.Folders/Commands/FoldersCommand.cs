//-----------------------------------------------------------------------
// <copyright file="InstanceCommand.cs" company="Atomia AB">
//     Copyright (c) Atomia AB. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using Atomia.Provisioning.Base;
using Atomia.Provisioning.Base.Exceptions;
using Atomia.Provisioning.Base.Module;
using Atomia.Provisioning.Modules.Common;
using System.IO;

namespace Atomia.Provisioning.Modules.Folders.Commands
{
    /// <summary>
    /// Command which handles Template instances.
    /// </summary>
    public class FoldersCommand : ModuleCommandSimpleBase
    {

        /// <summary>
        /// Service settings to be applied.
        /// </summary>
        private ModuleService newServiceSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceCommand"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="newServiceSettings">The new service settings.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="listDepth">The list depth.</param>
        public FoldersCommand(ModuleService service, ResourceDescription resource, ModuleService newServiceSettings, ModuleCommandType commandType, int listDepth)
            : base(service, resource, newServiceSettings, commandType, listDepth)
        {
            this.resource = resource;
            this.service = service;
            this.newServiceSettings = newServiceSettings;
            this.commandType = commandType;
            this.listDepth = listDepth;
        }

        /// <summary>
        /// Executes this  command.
        /// </summary>
        public override void Execute()
        {
            try
            {
                switch (this.commandType)
                {
                    case ModuleCommandType.Add:
                        this.ExecuteAdd(this.service);
                        break;
                    case ModuleCommandType.Modify:
                        this.ExecuteModify(this.service, this.newServiceSettings);
                        break;
                    case ModuleCommandType.Remove:
                        this.ExecuteRemove(this.service);
                        break;
                    case ModuleCommandType.List:
                        this.ExecuteList(this.service, this.listDepth);
                        break;
                    default:
                        throw ExceptionHelper.GetModuleException("ID400019", null, null);
                }

                this.status = ModuleCommandStatus.Executed;
            }
            catch (Exception e)
            {
                this.HandleException(e);
            }
        }

        /// <summary>
        /// Undoes this command.
        /// </summary>
        public override void Undo()
        {
            try
            {
                switch (this.commandType)
                {
                    case ModuleCommandType.Add:
                        this.ExecuteRemove(this.service);
                        break;
                    case ModuleCommandType.Modify:
                        this.ExecuteModify(this.newServiceSettings, this.service);
                        break;
                    case ModuleCommandType.Remove:
                        this.ExecuteAdd(this.service);
                        break;
                    default:
                        throw ExceptionHelper.GetModuleException("ID400019", null, null);
                }
            }
            catch (Exception e)
            {
                this.HandleException(e);
            }
        }

        /// <summary>
        /// Executes the remove.
        /// </summary>
        /// <param name="moduleService">The module service.</param>
        protected override void ExecuteRemove(ModuleService moduleService)
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

            if (Directory.Exists(folderFullPath))
            {
                Directory.Delete(folderFullPath, true);
            }
            else
            {
                //if folder dont exist nothing to do //throw ExceptionHelper.GetModuleException("ID422009", null, null);
            }

        }

        /// <summary>
        /// Executes the modify.
        /// </summary>
        /// <param name="oldService">The old service.</param>
        /// <param name="newService">The new service.</param>
        protected override void ExecuteModify(ModuleService oldService, ModuleService newService)
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

        /// <summary>
        /// Executes the add.
        /// </summary>
        /// <param name="moduleService">The module service.</param>
        protected override void ExecuteAdd(ModuleService moduleService)
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

        /// <summary>
        /// Validates the service.
        /// </summary>
        /// <param name="moduleService">The module service.</param>
        protected override void ValidateService(ModuleService moduleService)
        {
        }

        /// <summary>
        /// Calls the operation.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="operationArgument">The operation argument.</param>
        /// <returns></returns>
        public override string CallOperation(string operationName, string operationArgument)
        {
            string result = string.Empty;

            switch (operationName)
            {
                case "GetFullFolderPath":
                    {
                        result = resource["RootFolder"] + service["Name"];
                    }
                    break;
                default:
                    throw ExceptionHelper.GetModuleException("ID400019", null, null);
            }

            return result;

        }


    }
}