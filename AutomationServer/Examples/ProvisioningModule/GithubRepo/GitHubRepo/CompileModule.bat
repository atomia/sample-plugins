REM make .exe and rename it
c:\Python27\Scripts\cxfreeze.bat Atomia.Provisioning.Modules.GitHubRepository\GitHubRepository.py
copy "Atomia.Provisioning.Modules.GitHubRepository\dist\GitHubRepository.exe" "Atomia.Provisioning.Modules.GitHubRepository\dist\Atomia.Provisioning.Modules.GitHubRepository.exe" /y