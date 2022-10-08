rmdir /S /Q bin
CALL build.net35.bat Release
powershell -file sign.ps1
CALL build.installer.bat
powershell -file sign.installers.ps1
powershell -file sha1.ps1
