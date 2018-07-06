using System.Collections.Generic;
using System.Json;
using System;

namespace ckb
{
    public class Variable
    {
        public string name = "";
        public string type = "";
        public string defaultVal = "";
        public override string ToString()
        {
            return "{name='" + name + "',type='" + type + "',defaultVal='" + defaultVal + "'}";
        }
        public string size
        {
            get
            {
                switch (type)
                {
                    case "std::string":
                        return name + ".size() + 1 + sizeof(SizeS)";
                    default:
                        return "sizeof(" + type + ")";
                }
            }
        }
    }

    public struct Event
    {
        public string label;
        public string callableName;
        public Event(string callableName)
        {
            label = "LABEL: " + callableName;
            this.callableName = callableName;
        }
    }
    public class CKObject
    {
        public string name = "";

        // Name : Type
        public List<Variable> vars = new List<Variable>();

        // Label : Callable Name
        public List<Event> events = new List<Event>();

        public override string ToString()
        {
            string output = "{name=\"" + name + "\",vars=[";
            foreach (var var in vars)
            {
                output = output + var.ToString();
            }
            output = output + "],events=[";
            foreach (var Event in events)
            {
                output = output + Event.ToString();
            }
            output = output + "]}";
            return output;
        }

        private static byte c_UUID = 0x0000;

        private byte i_uuid = 0x0000;

        public static void initUUID(byte starting)
        {
            if (c_UUID > 0x0000)
            {
                Utils.PrintFatal("Reintializing UUID, aborting...");
            }

            c_UUID = starting;
        }

        public byte uuid
        {
            get
            {
                if (c_UUID < 0x0001)
                {
                    Utils.PrintFatal("UUIDs have not been intialized, aborting...");
                    return 0x0000;
                }
                if (i_uuid < 0x0001)
                {
                    i_uuid = c_UUID;
                    c_UUID++;
                }
                return i_uuid;
            }
        }

        public string uuidString
        {
            get
            {
                return "0x" + uuid.ToString("X4");
            }
        }
    }
}