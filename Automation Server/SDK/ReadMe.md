Automation server plugin SDK

1. Generally for developing and testing Automation server plugin next prerequisites should be installed on machine:
	- Installed Atomia Automation Server on local machine in bootstrap mode
	- Installed Microsoft .NET Framework 4.0
	- Installed Atomia Command Line tool or Installed AutomationServerClient, as testing tool.
	
2. Prepare your plugin project

	- For developing plugin in .NET you should have installed next prerequisites:		
			- Visual Studio 2010
			- .NET Framework 4.0
		
	Download AtomiaAutomationServer SDK.zip, unpack it and copy template you want to use.
	In order to choose appropriate template for your plugin, please read template specific readme file, where purpose and usage of template is described.
			
3. Programming provisioning plugin

	Generally plugin development can be accomplished in next steps:
	- Take appropriate template project and change it according instructions in template specific readme file.
	- Write plugin code
	- Change ServiceDescription.xml
	- Change ResourceDescription.xml
	- Build and install plugin
	
	Detailed instructions can be found in template specific readme file.
