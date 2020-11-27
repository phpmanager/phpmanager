Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.5/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.5/PHPManagerForIIS_x64.msi' `
-Checksum '4F81FA98E18B64D1388387BC8E534D8E41F28F39' -ChecksumType 'sha1' `
-Checksum64 'C0550804E601402A7DC607101C4D01B571D6BF9A' -ChecksumType64 'sha1'
