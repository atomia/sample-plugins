Automation server plugin SDK

Purpose of Automation server plugin SDK is to provide all what is needed for developing Automation Server plugin. 
Beside this readme file, here you can find 'AtomiaAutomationServer SDK.zip' which contains:

	- software components which are used and referenced by developing plugin projects - Lib folder
	- project templates as start point for developing plugin 
	- setup for testing environment where new developed plugins can be tested

1. How to start?

	- Download 'AtomiaAutomationServer SDK.zip' and unpack it to your working folder.
	- Choose which template you will use for plugin development. To determine which template is most adequate for plugin you plan to develop, please read template specific readme file, where purpose and usage of template is described.
	- Install all needed prerequisites before start to work
	- Copy template project to your working folder and follow instructions in template specific readme file.
	- As additional help examine appropriate working example which can be found in Examples folder of this git repository. Examples should be downloaded separately from github.

2. Prerequisites

	- Installed Atomia Automation server testing environment on local machine. It contains lite version of Atomia Automation Server, and as testing tools Atomia Command Line tool and Installed AutomationServerClient. Installation can be found in 'AtomiaAutomationServer SDK.zip'.
	- Installed Microsoft .NET Framework 4.0 - you can download it from http://msdn.microsoft.com/en-us/netframework/aa569263	
	- If you plan to develop plugin module in .NET then you will need Visual Studio 2010 installed.	
			
3. Generally provisioning plugin programming steps	
	
	- Take appropriate template project and change it according instructions in template specific readme file.
	- Write plugin code
	- Change ServiceDescription.xml
	- Change ResourceDescription.xml
	- Build and install plugin
	
	Detailed instructions for every step can be found in template specific readme file.