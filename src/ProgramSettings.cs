using System.Runtime.InteropServices;

namespace ckb
{
    class ProgramSettings
    {
        public bool engine;
        public bool verbose;
        public bool install;

        public string version = "0.1.0";
        
        public OSPlatform platform = OSPlatform.Windows;
    }
}