Automation server plugin SDK

1. Be sure you have .NET Framework 4.0 installed - you can download it from http://msdn.microsoft.com/en-us/netframework/aa569263

2. Download SQL Server Express from http://download.microsoft.com/download/D/1/8/D1869DEC-2638-4854-81B7-0F37455F35EA/SQLEXPRADV_x64_ENU.exe.

3. Install SQL Server Express with next command
	SQLEXPRADV_x64_ENU.exe /ACTION=Install /IACCEPTSQLSERVERLICENSETERMS /INSTANCEID=MSSQLSERVER /INSTANCENAME=MSSQLSERVER /QS /AGTSVCACCOUNT="NT Authority\Network Service" /SQLSVCACCOUNT="NT AUTHORITY\SYSTEM" /RSSVCACCOUNT="NT Authority\Network Service" /TCPENABLED=1 /SQLSYSADMINACCOUNTS=".\Administrator"

4. Download all files from this directory

5. Run setup.exe from bin subfolder. Now you have SDK and required apps to test examples and your plugins.

6. Try to compile and run some examples from: https://github.com/atomia/sample-plugins/tree/master/AutomationServer/Examples

7. Develop your plugins according to https://github.com/atomia/sample-plugins/tree/master/AutomationServer/SDK/ProjectTemplates