Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.11/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.11/PHPManagerForIIS_x64.msi' `
-Checksum '1E3E44EFBAA09B0F220597AF2F6ABDF2B20FDD60' -ChecksumType 'sha1' `
-Checksum64 '936F138456521FC959D869FA6A338CF9473959F5' -ChecksumType64 'sha1'
