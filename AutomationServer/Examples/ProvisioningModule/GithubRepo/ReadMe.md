Testing command line plugin example for provisioning folders

1. Install SDK, see: https://github.com/atomia/sample-plugins/blob/master/AutomationServer/SDK/ReadMe.md

2. Download all files from this directory

3. Run make.bat

	3.1 If you want to change example and test changes, you must have installed:
	
		- python 2.7 (http://www.python.org/download/releases/2.7.2/)
		
		- cx-freeze (http://sourceforge.net/projects/cx-freeze/files/4.2.3/cx_Freeze-4.2.3.win-amd64-py2.7.msi/download)
		
		After you made changes you should start makeWIthCompile.bat instead of make.bat
	
4. Open c:\Program Files (x86)\Atomia\AutomationServer\Common\resources.xml and set your github(you can make github account for free on github.com) login credential values instead of 'testgithubusername' and 'testgithubapitoken' for next properties:
	
		<property name="UserName">testgithubusername</property>
		<property name="APIToken">testgithubapitoken</property>
		after changes are made, reset iis with iisreset command

5. Try some of the following tests:
	
	5.1 Testing using atomia command line tool
		
		- Add service:
			
			windows cmdprompt:
				atomia service add --account 100000 --servicedata "{ \"name\" : \"GitHubRepository\", \"properties\" : { \"Name\" : \"test123\", \"Description\" : \"test123 description\"}}"
				
			This command will provide new service and will add new folder with given name 'test123'. As output from command you will get newly created service data serialized. Find logical id and remember it, because we will need it for modify and delete commands.
			In github you can check if new repository is created.
		
		- Modify service(NOT IMPLEMENTED YET):
			
			windows cmdprompt:
				atomia service modify --account 100000 --service "e4ed6bef-098e-4de4-9b9c-8ca7435809a8" --servicedata "{ \"properties\" : { \"Description\" : \"test Description is changed\"}}"
				
			This command will provide change of existing service i.e. will change description for repository. serviceId used in this command "67e9a578-7151-4569-93a0-a1c98c165855" is one we get during testing, you must replace it with logicalid you writed down during add command testing.
			You can check in github if description is changed.
			
		- Delete service:
		
			windows cmdprompt:
				atomia service delete --account 100000 --service "e4ed6bef-098e-4de4-9b9c-8ca7435809a8"
			
			This command will provide deleting of existing service with given service id. ServiceId used in this command "67e9a578-7151-4569-93a0-a1c98c165855" is one we get during testing, you must replace it with logicalid you writed down during add command testing.
			You can check in github if repository is realy deleted.
	
	5.2 Testing using AutomationServerClient.exe
	
		Start AutomationServerClient.exe. If you start AutomationServerClient for first time, you should set package which is used for work. From menu open File -> New Package, choose BasePackage and press AddPackage button.
		
		Right mouse click on empty area in service list, Add Child Service -> GitHubRepository, change name for repository because that is key property and press OK butoon. New service will appears in list, and you can check on github if new repository is added.
		NOT IMPLEMENTED CURRENTLY: Next you can try to edit service, right click on it, start Edit Service popup menu command, Edit service form will appear. Change parameter Description and click OK. After refreshing service tree, we can see that service is changed, now have new description. Also you can check if description for repository is changed in github.
		At last you can try to delete service, right click on it, start Delete Service popup menu command, prompt will appear, press yes, and service will be deleted. You can check if repository is deleted in github.
	