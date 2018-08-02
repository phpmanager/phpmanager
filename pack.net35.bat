CALL "%VS120COMNTOOLS%\vsvars32.bat"
CALL devenv .\Setup\PHPManagerSetup.vdproj /build %1
CALL devenv .\Setup\PHPManagerSetup64.vdproj /build %1