rmdir /S /Q bin
CALL build.net35.bat Release
CALL build.installer.bat
CALL sign.bat Release
