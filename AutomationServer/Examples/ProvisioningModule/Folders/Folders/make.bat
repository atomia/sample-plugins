c:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild "Atomia.Provisioning.Modules.Folders\Atomia.Provisioning.Modules.Folders.csproj" /p:Configuration=Release
REM example for command with full path: c:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild "c:\Projects\Atomia Plugin User Guide\Atomia Plugin Example\Automation Server Provisioning Plugin\Atomia.Provisioning.Module.Folders.sln" /p:Configuration=Release

copy "Atomia.Provisioning.Modules.Folders\bin\Release\Atomia.Provisioning.Modules.Folders.exe" "c:\Program Files (x86)\Atomia\AutomationServer\Common\Modules\" /y

REM transform ProvisioningDescription and Resources.xml files
REM 1. copy provisioning description and resource file to Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Original Files
copy "c:\Program Files (x86)\Atomia\AutomationServer\Common\ProvisioningDescriptions\ProvisioningDescription.xml" "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Original Files\" /y
copy "c:\Program Files (x86)\Atomia\AutomationServer\Common\Resources.xml" "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Original Files\" /y
REM 2. transform files
"c:\Program Files (x86)\Atomia\Common\Transformation\XmlTransformation.exe" Atomia.Provisioning.Modules.Folders\XmlFilesTransformation
REM 3. copy transformed provisioning and resource files to its location
copy "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\ProvisioningDescription.xml" "c:\Program Files (x86)\Atomia\AutomationServer\Common\ProvisioningDescriptions\ProvisioningDescription.xml" /y
copy "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Resources.xml" "c:\Program Files (x86)\Atomia\AutomationServer\Common\Resources.xml" /y


