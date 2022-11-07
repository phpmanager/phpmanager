rmdir /S /Q bin
CALL build.net35.bat Debug
IF %ERRORLEVEL% NEQ 0 goto failed

echo succeeded.
exit /b 0

:failed
echo failed.
exit /b 1
