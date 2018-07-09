using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Scriban;
using static ckb.Utils;

namespace ckb
{
    class Program
    {

        ProgramSettings settings = new ProgramSettings();

        static int Main(string[] args)
        {
            Program P = new Program();
            int code = P.Start(args);
            return code;
        }
        int Start(string[] args)
        {
            if (args.Length == 0)
            {
                PrintFatal("No Arguments Provided");
            }
            List<string> ToParse = new List<string>();
            List<string> NonOpts = new List<string>();

            // Simple Pre-Parsing and syntax checking
            foreach (string a in args)
            {
                // Eliminate 3+ dashes prefix
                if (a.Length > 2 && a.Substring(0, 3) == "---")
                {
                    PrintFatal("Error Parsing Argument '" + a + "': 3+ dash prefix");
                }

                if (a.Length > 1 && a.Substring(0, 2) == "--")
                {
                    ToParse.Add(a.Substring(2));
                    continue;
                }

                if (a.Substring(0, 1) == "-")
                {
                    foreach (char c in a.Substring(1))
                    {
                        Console.WriteLine(c);
                        ToParse.Add(c.ToString());
                    }
                    continue;
                }

                NonOpts.Add(a);
            }

            // Parse the Arguments
            bool shouldDie = false;
            foreach (string a in ToParse)
            {
                switch (a)
                {
                    case "engine":
                    case "e":
                        settings.engine = true;
                        break;
                    case "v":
                    case "verbose":
                        settings.verbose = true;
                        break;
                    case "i":
                    case "install":
                        settings.install = true;
                        break;
                    default:
                        PrintFatalSoft("Error Parsing Argument '" + a + "': unrecognized argument");
                        shouldDie = true;
                        break;
                }
                if (shouldDie)
                {
                    return -1;
                }
            }

            InitUtils(settings);

            // For now we only want one target, but it is a list for future compatibility
            if (NonOpts.Count > 1)
            {
                PrintFatal("Error Parsing Argument '" + NonOpts[1] + "': unrecognized extra argument");
                return -1;
            }
            string path = NonOpts[0];
            string dir = "";
            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string[] files = Directory.GetFiles(path, "*.ckproj");
                if (files.Length == 0)
                {
                    PrintFatal("No project configuration file in target directory(" + path + ")");
                }
                if (files.Length > 1)
                {
                    PrintWarning("More than 1 project configuration file in directory '" + path + "', using first entry(" + files[0] + ")");
                }
                dir = path;
                path = files[0];
            }
            dir = dir + "/";
            string[] parts = path.Split(".");
            if (parts[parts.Length - 1] != "ckproj")
            {
                PrintFatal("Target File '" + path + "' is not a project configuration file");
            }
            string configJSON = File.ReadAllText(path);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                settings.platform = OSPlatform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                settings.platform = OSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                settings.platform = OSPlatform.OSX;
            }
            else
            {
                settings.platform = OSPlatform.Windows;
                PrintWarning("Unknow Platform '" + RuntimeInformation.OSDescription + "' Using Windows");
            }

            ProjectSettings proj = new ProjectSettings();
            if (dir == "")
            {
                string[] dira = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                foreach (string a in dira)
                {
                    dir = dir + a;
                }
            }

            string confString = File.ReadAllText(path);
            proj.parse(confString, settings);
            PrintWarning(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            if (!proj.engine)
            {
                PrintWarning(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            }
            else
            {
                CKObject.initUUID(0x0001);
            }


            BuildSelection bs = new BuildSelection(dir);
            HeaderParser hp = new HeaderParser(bs);
            Dictionary<SourceFile, List<CKObject>> objDict = hp.Parse();
            if (settings.verbose)
            {
                foreach (KeyValuePair<SourceFile, List<CKObject>> pair in objDict)
                {
                    SourceFile file = pair.Key;
                    List<CKObject> list = pair.Value;
                    Console.WriteLine();

                    Console.WriteLine("File: " + file.name + "# of OBJs: " + list.Count);
                    foreach (CKObject obj in list)
                    {
                        Console.WriteLine(obj);
                    }

                    Console.WriteLine();
                }
            }

            CodeGenerator cg = new CodeGenerator(objDict);
            cg.generate(dir + "./generated/code/");
            string CMakeListsDir;
            string source = Path.GetFullPath(Environment.CurrentDirectory + "/" + dir);

            if (!proj.engine)
            {
                CMakeListsDir = dir + "./generated/";
                CMakeListsDir = Path.GetFullPath(CMakeListsDir);
                var CMakeLists = Template.Parse(@"
cmake_minimum_required(VERSION 3.5)
project(CKTestGame)

if(${CMAKE_MINOR_VERSION} GREATER 10)
cmake_policy(SET CMP0072 NEW)
endif()

#find_package(OpenGL REQUIRED)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
#set(CMAKE_CXX_EXTENSIONS OFF)

set(CMAKE_BUILD_TYPE Debug)

file(GLOB SRC ""{{src}}/*.cpp"")

include_directories({{include}})
#include_directories(${CMAKE_CURRENT_SOURCE_DIR}/../engine/src)
include_directories({{engine.include}})
link_directories({{engine.lib}})

add_executable(TestGame ${SRC})
#${CMAKE_BINARY_DIR}/engine/lib .a
#target_link_libraries(TestGame ${CMAKE_BINARY_DIR}/engine/libCKEngine.a glfw g3logger ${OPENGL_LIBRARIES} pthread dl)
target_link_libraries(TestGame CodekraftEngine)
#if(""${UNIX}"" AND NOT ""${APPLE}"")
#target_link_libraries(TestGame X11 Xrandr Xinerama Xi Xxf86vm Xcursor)
#endif()
#if(""${APPLE}"")
#FIND_LIBRARY(Cocoa Cocoa)
#FIND_LIBRARY(IOKit IOKit)
#FIND_LIBRARY(CoreDisplay CoreDisplay)
#target_link_libraries(TestGame ${Cocoa} ${IOKit} ${CoreDisplay})
#endif()
            ");
                var CMakeLists_o = CMakeLists.Render(new { src = Path.GetRelativePath(Path.GetFullPath(dir + "./generated/"), dir + "./src"), include = dir + "./include", engine = new { include = "/usr/local/include/CK/", lib = "/opt/CodekraftEngine/lib/" } });
                Directory.CreateDirectory(dir);
                var CMakeLists_p = Path.GetFullPath(CMakeListsDir + "/CMakeLists.txt");
                if (!File.Exists(CMakeLists_p) || (File.ReadAllText(CMakeLists_p) != CMakeLists_o))
                {
                    File.WriteAllText(CMakeLists_p, CMakeLists_o);
                }
            }
            else
            {
                CMakeListsDir = source;
            }

            Directory.CreateDirectory(Path.GetFullPath(dir + "./generated/cmake/"));
            Directory.SetCurrentDirectory(Path.GetFullPath(dir + "./generated/cmake/"));
            CMakeListsDir = Path.GetRelativePath(".", CMakeListsDir);
            ProcessStartInfo startInfo = new ProcessStartInfo("cmake", CMakeListsDir + " -GNinja")
            {
                CreateNoWindow = false
            };
            startInfo.Environment["WITH_CKB"] = "TRUE";
            startInfo.RedirectStandardOutput = true;
            Process cmake_p = Process.Start(startInfo);
            cmake_p.WaitForExit();
            if (cmake_p.ExitCode != 0)
            {
                Utils.PrintFatal("CMake did not exit cleaning, aborting...");
            }

            Console.WriteLine();

            ProcessStartInfo n_startInfo = new ProcessStartInfo("ninja", "")
            {
                CreateNoWindow = false
            };
            n_startInfo.Environment["WITH_CKB"] = "TRUE";
            //n_startInfo.RedirectStandardOutput = true;
            Process ninja_p = Process.Start(n_startInfo);
            ninja_p.WaitForExit();
            if (ninja_p.ExitCode != 0)
            {
                Utils.PrintFatal("Build error in ninja, aborting...");
            }

            if (proj.install)
            {
                n_startInfo.FileName = "sudo";
                n_startInfo.Arguments = "ninja install";
                Process ninjai_p = Process.Start(n_startInfo);
                ninjai_p.WaitForExit();
                if (ninjai_p.ExitCode != 0)
                {
                    Utils.PrintFatal("Error while installing, aborting...");
                }
            }

            Console.WriteLine();
            return 0;
        }
    }
}
