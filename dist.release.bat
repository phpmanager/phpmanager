rmdir /S /Q bin
CALL build.net35.bat Release
CALL build.installer.bat
powershell -file sign.ps1
