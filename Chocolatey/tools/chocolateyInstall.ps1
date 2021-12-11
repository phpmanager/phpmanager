Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.7/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.7/PHPManagerForIIS_x64.msi' `
-Checksum '9AE196C06BE98C044C1A0BB9D85C6D0729EAAE21' -ChecksumType 'sha1' `
-Checksum64 '25EA786A271C9754DF33BE86E638C4E1EA3EAD63' -ChecksumType64 'sha1'
