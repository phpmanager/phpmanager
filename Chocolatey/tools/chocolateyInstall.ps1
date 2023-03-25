Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.12/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.12/PHPManagerForIIS_x64.msi' `
-Checksum '826E43583E9D9FEFC50A637E6A0857E2CBBB8828' -ChecksumType 'sha1' `
-Checksum64 'FDA0E343201B1DBCC9BBA5AADD656B8C0B28B34C' -ChecksumType64 'sha1'
