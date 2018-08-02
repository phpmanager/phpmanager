rmdir /S /Q bin
CALL build.net35.bat Release
CALL pack.net35.bat Release
CALL build.net4x.bat Release
CALL pack.net4x.bat Release
CALL sign.bat Release