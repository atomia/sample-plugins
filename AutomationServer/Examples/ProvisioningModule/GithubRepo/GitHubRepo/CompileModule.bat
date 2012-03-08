REM make .exe and rename it
c:\Python27\Scripts\cxfreeze.bat GitHubRepository\GitHubRepository.py
copy "GitHubRepository\dist\GitHubRepository.exe" "GitHubRepository\dist\Atomia.Provisioning.Modules.GitHubRepository.exe" /y