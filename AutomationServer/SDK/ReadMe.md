Automation server plugin SDK

1. Be sure you have .NET Framework 4.0 installed - you can download it from http://msdn.microsoft.com/en-us/netframework/aa569263

2. Be sure you have installed IIS with support for .net and asp

3. Download SQL Server Express from http://download.microsoft.com/download/D/1/8/D1869DEC-2638-4854-81B7-0F37455F35EA/SQLEXPRADV_x64_ENU.exe.

4. Install SQL Server Express with next command
	SQLEXPRADV_x64_ENU.exe /ACTION=Install /IACCEPTSQLSERVERLICENSETERMS /INSTANCEID=MSSQLSERVER /INSTANCENAME=MSSQLSERVER /QS /AGTSVCACCOUNT="NT Authority\Network Service" /SQLSVCACCOUNT="NT AUTHORITY\SYSTEM" /RSSVCACCOUNT="NT Authority\Network Service" /TCPENABLED=1 /SQLSYSADMINACCOUNTS=".\Administrator"

5. Be sure you have installed microsoft message queue
	
6. in command prompt execute respectively:

	C:\Windows\Microsoft.NET\Framework\v4.0.30319>aspnet_regiis.exe -u
	C:\Windows\Microsoft.NET\Framework\v4.0.30319>aspnet_regiis.exe -i	
	iisreset

7. It should be checked if Handler Mappings for UCP have module for SVC enabled.
	If there is no such item, follow this solution:
	You have to run servicemodelreg -i as an administrator using Command prompt (you can find it in <WINDOWSDRIVE>:\Windows\Microsoft.NET\Framework\v3.0\Windows Communication Foundation).
	
8. Download all files from this directory

9. Run setup.exe. Now you have SDK and required apps to test examples and develop and test your plugins.

10. Make sure, C:\Program Files (x86)\Atomia\AutomationServerCmdClient\atomia.conf looks like this:
		[Automation Server API]
		username=
		password=
		url=http://localhost/CoreAPI.svc?wsdl
		bootstrap=true

11. Try to compile and run some examples from: https://github.com/atomia/sample-plugins/tree/master/AutomationServer/Examples

12. If you get error message similar to: Could not load type 'System.ServiceModel.Activation.HttpModule' from assembly 'System.ServiceModel, Version=3.0.0.0 ... ',
	go to to folder %windir%\Microsoft.NET\Framework\v4.0.30319 or %windir%\Microsoft.NET\Framework64\v4.0.30319 (on a 64-bit machine) and start next command:
	aspnet_regiis.exe /iru	

13. Develop your plugins according to https://github.com/atomia/sample-plugins/tree/master/AutomationServer/SDK/ProjectTemplates