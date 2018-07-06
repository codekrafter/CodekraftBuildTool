namespace ckb
{
    class NinjaGenerator : Generator
    {

        public override void generate(string outputFolder)
        { }

        public override string getExtension() {
            return "";
        }

        public override string[] getPlatforms() { 
            return null;
        }

        public override bool checkAvailability() {
            System.Diagnostics.Process.Start("echo","test abc");
            return true;
        }
    }
}