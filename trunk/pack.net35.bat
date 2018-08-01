CALL "%VS120COMNTOOLS%\vsvars32.bat"
CALL devenv .\Setup\PHPManagerSetup.vdproj /build Release
CALL devenv .\Setup\PHPManagerSetup64.vdproj /build Release