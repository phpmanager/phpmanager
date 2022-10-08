del bin\Release\PHPManagerForIIS.msi
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild .\Setup\PHPManagerForIIS.wixproj /t:Rebuild /property:Configuration=Release /property:Platform=x86
copy bin\Release\PHPManagerForIIS.msi .\PHPManagerForIIS_x86.msi
del bin\Release\PHPManagerForIIS.msi
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild .\Setup\PHPManagerForIIS.wixproj /t:Rebuild /property:Configuration=Release /property:Platform=x64
copy bin\Release\PHPManagerForIIS.msi .\PHPManagerForIIS_x64.msi
