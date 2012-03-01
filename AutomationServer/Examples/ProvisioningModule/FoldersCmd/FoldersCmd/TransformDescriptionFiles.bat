REM transform ProvisioningDescription and Resources.xml files
REM 1. copy provisioning description and resource file to Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Original Files
copy "c:\Program Files (x86)\Atomia\AutomationServer\Common\ProvisioningDescriptions\ProvisioningDescription.xml" "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Original Files\" /y
copy "c:\Program Files (x86)\Atomia\AutomationServer\Common\Resources.xml" "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Original Files\" /y
REM 2. transform files
"c:\Program Files (x86)\Atomia\Common\Transformation\XmlTransformation.exe" Atomia.Provisioning.Modules.Folders\XmlFilesTransformation
REM 3. copy transformed provisioning and resource files to its location
copy "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\ProvisioningDescription.xml" "c:\Program Files (x86)\Atomia\AutomationServer\Common\ProvisioningDescriptions\ProvisioningDescription.xml" /y
copy "Atomia.Provisioning.Modules.Folders\XmlFilesTransformation\Resources.xml" "c:\Program Files (x86)\Atomia\AutomationServer\Common\Resources.xml" /y
