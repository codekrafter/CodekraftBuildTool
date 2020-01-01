using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace ckb
{
    public class HeaderParser
    {
        BuildSelection bs;

        public HeaderParser(BuildSelection bsa)
        {
            bs = bsa;
        }

        public Dictionary<SourceFile, List<CKObject>> Parse()
        {
            Dictionary<SourceFile, List<CKObject>> allObjs = new Dictionary<SourceFile, List<CKObject>>();
            foreach (SourceFile file in bs.headers)
            {
                if (file.type != FileType.CPP_HEADER && file.type != FileType.C_HEADER)
                {
                    Utils.PrintWarning("File '" + file.path + "/" + file.name + "' is not a header file(it is a " + file.type + "), but is in headers list. Ignoring it");
                    continue;
                }

                int lineNum = 1;
                List<CKObject> objs = new List<CKObject>();
                CKObject currentObj = new CKObject();
                ushort nextItem = 0;
                Dictionary<string, string> properties = new Dictionary<string, string>();
                foreach (string line in File.ReadLines(file.path + "/" + file.name))
                {
                    if (nextItem > 0)
                    {
                        if (nextItem == 1)
                        {
                            if (!line.Contains("class"))
                            {
                                Utils.PrintError("Found CKClass Macro, but did not find class declaration in next line" + "\n" +
                                "File: " + file.name + " Line: " + lineNum);
                            }
                            int classI = line.IndexOf("class", StringComparison.CurrentCultureIgnoreCase);
                            string postClass = line.Substring(classI + "class".Length);
                            postClass = postClass.Trim();
                            string name = "";
                            if (postClass.Count(x => x == ':') == 0)
                            {
                                name = postClass;
                            }
                            else
                            {
                                name = postClass.Substring(0, postClass.IndexOf(":", StringComparison.CurrentCulture)).Trim();
                            }
                            if (name.Length < 1)
                            {
                                Utils.PrintError("Error Parsing Line for Class Name: '" + line + "'");
                                name = "ERROR";
                            }
                            currentObj.name = name;
                            nextItem = 0;
                        }
                        else if (nextItem == 2)
                        {
                            string[] parts = line.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            bool redef = false;
                            currentObj.vars.ForEach((obj) => redef |= (obj.name == parts[1]));
                            if (redef)
                            {
                                Utils.PrintError("Redefinition of " + currentObj.name + "." + parts[1] + ". Overwriting");
                                Utils.PrintError("File: " + file.path + "/" + file.name + " Line: " + lineNum);
                            }
                            Variable cvar = new Variable();
                            int name_offset = 1;
                            if (parts[0].Trim() == "unsigned" || parts[0].Trim() == "signed")
                            {
                                cvar.type = parts[0] + " " + parts[1];
                                //string[] newA = new string[parts.Length - 1];
                                //Array.Copy(parts, newA, parts.Length - 1);
                                //Array.Resize(ref parts, newA.Length);
                                parts = parts.Skip(1).ToArray();
                            }
                            else
                            {
                                string type = parts[0];
                                while (type.Contains("<") && !type.Contains(">"))
                                {
                                    if (name_offset > parts.Length - 1)
                                    {
                                        name_offset--;
                                        break;
                                    }

                                    type += parts[name_offset];

                                    name_offset++;
                                }
                                if(type.Contains("<"))
                                {
                                cvar.templateType = type.Split('<', '>')[1].Trim();

                                cvar.type = type.Split('<')[0].Trim();
                                } else
                                {
                                    cvar.type = type.Trim();
                                }

                                //string[] globalTypes = {"uint_8", "size_t", "unsigned", "signed", "int", "bool", "char", "long", "short"};

                                /* if(cvar.templateType.Length > 0 && !cvar.templateType.Contains("::") && !globalTypes.Any(cvar.templateType.Contains))
                                {
                                    cvar.templateType = "ck::" + cvar.templateType;
                                }

                                if(!cvar.type.Contains("::") && !globalTypes.Any(cvar.type.Contains))
                                {
                                    cvar.type = "ck::" + cvar.type;
                                }*/

                                cvar.properties = properties;
                            }
                            cvar.name = parts[name_offset];
                            if (parts.Length > 3)
                            {
                                cvar.defaultVal = parts[3].Substring(0, parts[3].Length - 1);
                            }
                            else
                            {
                                cvar.name = cvar.name.Substring(0, cvar.name.Length - 1);
                            }
                            currentObj.vars.Add(cvar);

                            nextItem = 0;
                        }
                        else if (nextItem == 3)
                        {
                            string[] parts = line.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            bool redef = false;
                            currentObj.events.ForEach((obj) => redef |= (obj.callableName == parts[1]));
                            if (redef)
                            {
                                Utils.PrintError("Redefinition of " + currentObj.name + "." + parts[1] + ". Overwriting");
                                Utils.PrintError("File: " + file.path + "/" + file.name + " Line: " + lineNum);
                            }
                            Event cevent = new Event(parts[1]);
                            currentObj.events.Add(cevent);

                            nextItem = 0;
                        }
                        else
                        {
                            Utils.PrintError("Unknown Macro ID " + nextItem + ", ignoring it");
                            nextItem = 0;
                        }
                    }
                    else
                    {
                        string macro;
                        try
                        {
                            macro = checkLine(line);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            macro = "";
                        }

                        if (macro.Length > 0)
                        {
                            string[] splitMacro = macro.Split('(', ')').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            string cmacro = splitMacro[0];//macro.Substring(0, macro.IndexOf("(", StringComparison.CurrentCulture));
                            if (cmacro == "CKClass")
                            {
                                if (currentObj.name.Length > 0)
                                {
                                    objs.Add(currentObj);
                                    currentObj = new CKObject();
                                }
                                nextItem = 1;
                            } /*
                            else if (cmacro == "CKStruct")
                            {
                            }*/
                            else if (cmacro == "CKProperty")
                            {
                                nextItem = 2;
                                properties.Clear();
                                string[] props = splitMacro.Where( x => x.Contains("=")).ToArray();
                                foreach(string prop in props)
                                {
                                    string[] split = prop.Split('=');
                                    string lhs = split[0];
                                    string rhs = split[1];

                                    properties.Add(lhs, rhs);
                                }
                            }
                            else if (cmacro == "CKEvent")
                            {
                                nextItem = 3;
                            }
                            else
                            {
                                Utils.PrintError("Unknown CKMacro '" + cmacro + "', ignoring it");
                                nextItem = 0;
                            }
                        }
                    }

                    lineNum++;
                }

                if (currentObj.name.Length > 0)
                {
                    objs.Add(currentObj);
                    currentObj = new CKObject();
                }

                if (objs.Count > 0)
                {
                    allObjs.Add(file, objs);
                }
            }
            return allObjs;
        }

        string checkLine(string line)
        {
            line = line.Trim();
            if (line.Length < 3)
            {
                return "";
            }
            bool is_macro = false;
            is_macro = (line.Substring(0, 2) == "CK");
            is_macro = is_macro && (line.Count(x => x == '(') > 0);
            if (is_macro == false)
                return "";
            is_macro = is_macro && (line.Substring(0, line.IndexOf("(", StringComparison.CurrentCulture)).Count(x => x == ':') == 0);
            is_macro = is_macro && (line.Substring(0, 8) != "CKEngine");
            return is_macro ? line : "";
        }
    }
}
