CALL "%VS120COMNTOOLS%\vsvars32.bat"
CALL devenv .\Setup\PHPManagerSetup8.vdproj /build Release
CALL devenv .\Setup\PHPManagerSetup8_64.vdproj /build Release