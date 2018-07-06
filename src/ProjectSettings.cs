using System.Json;
using System;

namespace ckb
{
    class ProjectSettings
    {
        public string projName; // Name of Project
        public float engVersion; // Version of Target Engine (If building as then then it must be the same as project version)
        public float projVersion; // Version of Project (User Defined)
        public bool engine; // Whether to build as engine or not ( generate only)
        public bool install; // Whether to install executables or not

        public void parse(string jsonString, ProgramSettings settings)
        {
            JsonValue json = JsonValue.Parse(jsonString);
            if (json.ContainsKey("engine"))
            {
                engine = json["engine"];
            }
            else
            {
                engine = settings.engine;
            }

            if (!json.ContainsKey("name") || !json.ContainsKey("projectVersion") || (!engine && !json.ContainsKey("engineVersion")))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Project Settings File does not have the required fields");
                Console.ResetColor();
                Environment.Exit(-1);
            }

            if (engine && !json.ContainsKey("engineVersion"))
            {
                json["engineVersion"] = json["projectVersion"];
            }

            projName = (string)json["name"];
            engVersion = (float)json["engineVersion"];
            projVersion = (float)json["projectVersion"];

            if (json.ContainsKey("install"))
            {
                install = (bool)json["install"];
            }
            else
            {
                install = settings.install;
            }

        }
    }
}