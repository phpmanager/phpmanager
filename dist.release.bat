rmdir /S /Q bin
CALL build.net35.bat Release
IF %ERRORLEVEL% NEQ 0 goto failed
powershell -ExecutionPolicy Bypass -file sign.ps1
IF %ERRORLEVEL% NEQ 0 goto failed
CALL build.installer.bat
IF %ERRORLEVEL% NEQ 0 goto failed
powershell -ExecutionPolicy Bypass -file sign.installers.ps1
IF %ERRORLEVEL% NEQ 0 goto failed
powershell -ExecutionPolicy Bypass -file verify.ps1
IF %ERRORLEVEL% NEQ 0 goto failed
powershell -ExecutionPolicy Bypass -file sha1.ps1
IF %ERRORLEVEL% NEQ 0 goto failed

echo succeeded.
exit /b 0

:failed
echo failed.
exit /b 1
