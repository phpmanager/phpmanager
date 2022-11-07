Install-ChocolateyPackage -PackageName 'PHP Manager 2 for IIS' -FileType 'msi' -SilentArgs '/quiet' `
-File 'https://github.com/phpmanager/phpmanager/releases/download/v2.9/PHPManagerForIIS_x86.msi' `
-File64 'https://github.com/phpmanager/phpmanager/releases/download/v2.9/PHPManagerForIIS_x64.msi' `
-Checksum 'F485FD5E4112F0483F9A8FCE8630661A5C625576' -ChecksumType 'sha1' `
-Checksum64 '3FAFCB4999E6293CA5B60FDFA49A534686831C09' -ChecksumType64 'sha1'
