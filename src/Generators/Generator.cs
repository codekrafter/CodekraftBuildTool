namespace ckb
{
    abstract class Generator
    {
        public abstract void generate(string outputFolder);

        public abstract string getExtension();

        public abstract string[] getPlatforms();

        public abstract bool checkAvailability();
    }
}