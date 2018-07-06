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

echo "Building Project"
echo

if [[ OS -eq 1 ]]; then
    dotnet publish -v q -c Release -r linux-x64
    rc=$?
elif [[ OS -eq 2 ]]; then
    dotnet publish -v q -c Release -r osx-x64
    rc=$?
else 
    echo "Unknown OS Code: $OS"
    exit -1
fi

if [[ $rc != 0 ]]; then
    echo
    echo "Building exited with $rc, Not completing install"
    exit $rc
fi

echo
echo "Done Building Project"
echo


./install.sh
