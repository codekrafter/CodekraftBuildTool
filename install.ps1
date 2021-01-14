#!/bin/bash

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


if($OS -eq 1 -Or $OS -eq 2) {
	bash ./install.sh
	exit
}

echo "Please Install Manually files in bin/"
exit

cd bin 

cd Release

cd netcoreapp2.0

cd win-x64

echo "Installing Files..."

mkdir -p /usr/local/lib/ckbuild/

echo "    Installing Executable"
sudo cp ckbuild /usr/local/lib/ckbuild/

echo "    Installing Required Linux Shared Libraries"
sudo cp ./*.so /usr/local/lib/ckbuild/

echo "    Installing Required Mac Shared Libraries"
sudo cp ./*.dylib /usr/local/lib/ckbuild/

echo "    Installing Required Windows Shared Libraries"
sudo cp ./*.dll /usr/local/lib/ckbuild/

echo "    Installing non-binary files (.json and .pdb)"
#sudo cp ./*.txt /usr/local/lib/ckbuild/
sudo cp ./*.json /usr/local/lib/ckbuild/
sudo cp ./*.pdb /usr/local/lib/ckbuild/

echo "Done Installing Files"

echo

echo "Creating Symlinks..."

echo "    Creating ckbuild Symlink"
sudo ln -sf /usr/local/lib/ckbuild/ckbuild /usr/local/bin/ckbuild

echo "    Creating ckb Symlink"
sudo ln -sf /usr/local/lib/ckbuild/ckbuild /usr/local/bin/ckb

echo "Done Creating Symlinks"

echo

echo "Setting Permissions for..."

echo "    ckbuild"
sudo chmod +xr /usr/local/bin/ckbuild

echo "    ckb"
sudo chmod +xr /usr/local/bin/ckb

echo "Done Setting Permissions"

echo

echo "Starting Dry Run to generate caches"
ckbuild > /dev/null
echo "Finished Dry Run"

echo
echo
echo

echo "Installed Codekraft Build Utility to /usr/local/lib"
echo "Use 'ckbuild' or 'ckb' to Run It"
echo