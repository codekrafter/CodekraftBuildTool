echo "Detecting Operating System..."
echo ""
$OS=-1
If ( "$($ENV:OS)" -ne $null -Or $IsWindows) {
    $OS = 3
} ElseIf ($IsLinux) {
    $OS = 1
} ElseIf ($IsMacOS) {
	$OS = 2
} Else {
    echo "Unable to determine OS"
    exit -1
}

echo "Done Detecting Operating"

echo "Building Project"
echo ""

If ( $OS -eq 1 ) {
    dotnet publish -v q -c Release -r linux-x64
    $rc = $LastExitCode
} ElseIf ($OS -eq 2) {
    dotnet publish -v q -c Release -r osx-x64
    $rc = $LastExitCode
} ElseIf ($OS -eq 3) {
	dotnet publish -v q -c Release -r win-x64
	$rc = $LastExitCode
} Else {
    echo "Unknown OS Code $OS"
    exit -1
}

if ($rc -ne 0 ) {
    echo ""
    echo "Building exited with Code $rc, Not completing install"
    exit $rc
}

echo ""
echo "Done Building Project"
echo ""


./install.ps1