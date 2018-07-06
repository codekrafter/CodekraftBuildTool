using System;
using System.Collections.Generic;

namespace ckb
{
    public enum FileType
    {
        PROJ_CONFIG,
        CPP_SOURCE,
        CPP_HEADER,
        C_SOURCE,
        C_HEADER
    }
    public struct SourceFile
    {
        public string path;
        public string name;
        public FileType type;
        public SourceFile(string p)
        {
            List<string> parts = new List<string>(p.Split("/", StringSplitOptions.RemoveEmptyEntries));
            path = string.Join("/", parts.GetRange(0, parts.Count - 1)) + '/';
            name = parts[parts.Count - 1];

            type = FileType.CPP_SOURCE;
        }

        public SourceFile(string p, FileType t)
        {
            List<string> parts = new List<string>(p.Split("/", StringSplitOptions.RemoveEmptyEntries));
            path = string.Join("/", parts.GetRange(0, parts.Count - 1));
            name = parts[parts.Count - 1];

            type = t;
        }
    }
}
