c:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild "Atomia.Provisioning.Modules.Folders\Atomia.Provisioning.Modules.Folders.csproj" /p:Configuration=Release
REM example for command with full path: c:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild "c:\Projects\Atomia Plugin User Guide\Atomia Plugin Example\Automation Server Provisioning Plugin\Atomia.Provisioning.Module.Folders.sln" /p:Configuration=Release

copy "Atomia.Provisioning.Modules.Folders\bin\Release\Atomia.Provisioning.Modules.Folders.exe" "c:\Program Files (x86)\Atomia\AutomationServer\Common\Modules\" /y


