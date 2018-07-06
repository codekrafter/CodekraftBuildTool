#!/bin/bash

echo "Detecting Operating System..."
echo
OS=-1
if [[ "$OSTYPE" == "linux-gnu" ]]; then
       OS=1
elif [[ "$OSTYPE" == "darwin"* ]]; then
        OS=2
elif [[ "$OSTYPE" == "cygwin" ]]; then
        echo "Please Use Windows Scripts"
        OS=1 
        exit -1
elif [[ "$OSTYPE" == "msys" ]]; then
        echo "Please Use Windows Scripts"
        OS=1
        exit -1
elif [[ "$OSTYPE" == "win32" ]]; then
        echo "Please Use Windows Scripts"
        exit -1
elif [[ "$OSTYPE" == "freebsd"* ]]; then
        echo "FreeBSD is not officially supported"
        OS=1
else
        echo "Unknown Operating System, please Submit Ticket, Your Operating System is '$OSTYPE'"
        exit -1
fi

echo "Done Detecting Operating"


cd bin 

cd Release

cd netcoreapp2.0

if [[ OS -eq 1 ]]; then
    cd linux-x64
elif [[ OS -eq 2 ]]; then
    cd osx-x64
else 
    echo "Unknown OS Code: $OS"
    exit -1
fi

echo "Installing Files..."

sudo mkdir -p /usr/local/lib/ckbuild/

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