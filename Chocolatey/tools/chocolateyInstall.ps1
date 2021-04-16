Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.6/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.6/PHPManagerForIIS_x64.msi' `
-Checksum 'BBAB59DF0800E0CD35A1C2CA19D9773E5C20DC8E' -ChecksumType 'sha1' `
-Checksum64 '7FF3504E7DB1DB309AC5C290B6CBB7E2CFF6D006' -ChecksumType64 'sha1'
