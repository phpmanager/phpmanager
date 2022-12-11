Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.10/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.10/PHPManagerForIIS_x64.msi' `
-Checksum '75538963E6A4CB7DDA31841ACF44E238E91E60C0' -ChecksumType 'sha1' `
-Checksum64 'BCF88FE12AE7125DC879D5170D8AFA3313DEE13C' -ChecksumType64 'sha1'
