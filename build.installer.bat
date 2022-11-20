del bin\Release\PHPManagerForIIS.msi
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild .\Setup\PHPManagerForIIS.wixproj /t:Rebuild /property:Configuration=Release /property:Platform=x86
IF %ERRORLEVEL% NEQ 0 goto failed
copy bin\Release\PHPManagerForIIS.msi .\PHPManagerForIIS_x86.msi
del bin\Release\PHPManagerForIIS.msi
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild .\Setup\PHPManagerForIIS.wixproj /t:Rebuild /property:Configuration=Release /property:Platform=x64
IF %ERRORLEVEL% NEQ 0 goto failed
copy bin\Release\PHPManagerForIIS.msi .\PHPManagerForIIS_x64.msi

echo succeeded.
exit /b 0

:failed
echo failed.
exit /b 1
