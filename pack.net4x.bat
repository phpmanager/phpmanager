CALL "%VS120COMNTOOLS%\vsvars32.bat"
CALL devenv .\Setup\PHPManagerSetup8.vdproj /build %1
CALL devenv .\Setup\PHPManagerSetup8_64.vdproj /build %1