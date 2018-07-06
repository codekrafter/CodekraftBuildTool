using System.IO;
using System.Collections.Generic;
using System;

namespace ckb
{
    public class BuildSelection
    {
        public BuildSelection(string dir)
        {
            // Generate Source File List, also generates headers list, for generating CKObjects
            files = recurseGetSRC(dir + "/src/");

            includePaths.Add(dir + "/include/");
            includePaths.Add(dir + "/src/");

            libPaths.Add(dir + "/lib/");

            rootDir = Path.GetRelativePath(".", dir);
        }

        private List<SourceFile> recurseGetSRC(string path)
        {
            List<string> fs = new List<string>(Directory.GetFiles(path));
            List<string> ds = new List<string>(Directory.GetDirectories(path));
            List<SourceFile> ofs = new List<SourceFile>();
            foreach (string f in fs.ToArray())
            {
                string[] parts = f.Split(".", StringSplitOptions.RemoveEmptyEntries);
                if (parts[parts.Length - 1] == "cpp")
                {
                    ofs.Add(new SourceFile(f));
                }

                if (parts[parts.Length - 1] == "c")
                {
                    ofs.Add(new SourceFile(f, FileType.C_SOURCE));
                }

                if (parts[parts.Length - 1] == "hpp")
                {
                    headers.Add(new SourceFile(f, FileType.CPP_HEADER));
                }

                if (parts[parts.Length - 1] == "h")
                {
                    headers.Add(new SourceFile(f, FileType.C_HEADER));
                }
            }

            foreach (string d in ds.ToArray())
            {
                List<SourceFile> nfs = recurseGetSRC(d);
                ofs.AddRange(nfs);
            }

            return ofs;
        }
        public List<SourceFile> files = new List<SourceFile>();

        public List<SourceFile> headers = new List<SourceFile>();

        public List<string> includePaths = new List<string>();

        public List<string> libPaths = new List<string>();

        public string rootDir;
    }
}