using System.Runtime.InteropServices;

namespace ckb
{
    class ProgramSettings
    {
        public bool engine;
        public bool verbose;
        public bool install;
        
        public OSPlatform platform = OSPlatform.Windows;
    }
}