Testing command line plugin example for provisioning folders

1. Install SDK, see: https://github.com/atomia/sample-plugins/blob/master/AutomationServer/SDK/ReadMe.md

2. Download all files from this directory

3. Run make.bat

4. Try some of the following tests:
	
	4.1 Testing using atomia command line tool
		
		- Add service:
			
			windows cmdprompt:
				atomia service add --account 100000 --servicedata "{ \"name\" : \"Folders\", \"properties\" : { \"Name\" : \"test123\", \"Description\" : \"test123 description\"}}"
				
			This command will provide new service and will add new folder with given name 'test123'. As output from command you will get newly created service data serialized. Find logical id and remember it, because we will need it for change and delete commands.
			We can check root folder, if you didn't change it in ResourceDescription.xml file, it is c:\TestProvisioningExample\, you will find folder 'test123' created.
		
		- Modify service:
			
			windows cmdprompt:
				atomia service modify --account 100000 --service "67e9a578-7151-4569-93a0-a1c98c165855" --servicedata "{ \"properties\" : { \"Name\" : \"testFolderWithChangedName\"}}"
				
			This command will provide change of existing service i.e. will rename existing folder 'test123' to 'testFolderWithChangedName'. serviceId used in this command "67e9a578-7151-4569-93a0-a1c98c165855" is one we get during testing, you must replace it with logicalid you writed down during add command testing.
			We can check our root folder and we will see that folder name is changed. So, change is provisioned. 
			
		- Delete service:
		
			windows cmdprompt:
				atomia service delete --account 100000 --service "67e9a578-7151-4569-93a0-a1c98c165855"
			
			This command will provide deleting of existing service with given service id. ServiceId used in this command "67e9a578-7151-4569-93a0-a1c98c165855" is one we get during testing, you must replace it with logicalid you writed down during add command testing.
			You can check if folder is realy deleted if you open our root folder. So, provisioning for delete service is passed succesfully.
	
	4.2 Testing using AutomationServerClient.exe
	
		Start AutomationServerClient.exe. If you start AutomationServerClient for first time, you should set package which is used for work. From menu open File -> New Package, choose BasePackage and press AddPackage button.
		
		Right mouse click on empty area in service list, Add Child Service -> Folders, change name for folder because that is key property and press OK butoon. New service will appears in list, and you can check in root folder, if you didn't change it in ResourceDescription.xml file, it is c:\TestProvisioningExample\, you will find folder with the same name as created service. 
		Next you can try to edit service, right click on it, start Edit Service popup menu command, Edit service form will appear. Change parameter Name, which represent name for folder and click OK. After refreshing service tree, we can see that service is changed, now have new name. Also we can check our root folder and we will see that folder name is changed. So, change is provisioned.
		At last you can try to delete service, right click on it, start Delete Service popup menu command, prompt will appear, press yes, and service will be deleted. You can check if folder is realy deleted if you open our root folder. So, provisioning for delete service is passed succesfully.
	