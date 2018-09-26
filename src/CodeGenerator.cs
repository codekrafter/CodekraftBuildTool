using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Scriban;

namespace ckb
{
    public class CodeGenerator
    {
        Dictionary<SourceFile, List<CKObject>> filesDict;
        public CodeGenerator(Dictionary<SourceFile, List<CKObject>> fd)
        {
            filesDict = fd;
        }

        public void generate(string dir)
        {
            // Cleanse Path
            string path = Environment.CurrentDirectory + "/" + dir;
            path = Path.GetRelativePath(".", path);
            List<CKObject> totalObjs = new List<CKObject>();
            List<string> totalIncludes = new List<string>();

            foreach (var pair in filesDict)
            {
                // Retrive file and objects for easy access
                SourceFile file = pair.Key;
                List<CKObject> objs = pair.Value;
                totalObjs.AddRange(objs);
                totalIncludes.Add(Path.GetFileNameWithoutExtension(file.name) + ".generated.hpp");
                var header = Template.Parse(@"
/*
 * Generated Code for file '{{file.name}}' by Codekraft Build Tool
 * Do not edit
 */

#include ""Assets/Serialization.hpp""
#include ""{{ include_file }}""
using namespace ck;

namespace ckg
{

{{ for obj in objects -}}

struct {{obj.name}}S : AssetS
{
    /*
     * Variables, one-to-one to real class
     */

    {{ for var in obj.vars }}
    {{ var.type }} {{ var.name }} {{-if var.default_val | string.size > 0}} = {{ var.default_val }} {{- end }};
    {{end}}

    /*
     * Serialization Functions, required for serialization to work,
     * and to convert between normal class ({{obj.name}})
     */
    {{obj.name}}S();
    {{obj.name}}S({{obj.name}} *{{obj.name | string.slice 0 1 | string.downcase}});
    virtual {{obj.name}} *asset() override;
    virtual DatSize save() override;
    virtual void load(unsigned char *data, size_t size) override;

    const virtual unsigned char getUUID() override;
    //virtual {{obj.name}}S *clone() override;
};

{{- end }}
}

/*
 * End of File
 */
");
                var source = Template.Parse(@"
#include ""{{- include_name -}}""
using namespace ck;

namespace ckg
{
{{ for obj in objects -}}

{{obj.name}}S::{{obj.name}}S(){};

{{obj.name}}S::{{obj.name}}S({{obj.name}} *obj)
{

    {{ for var in obj.vars }}
    {{ var.name }} = obj->{{ var.name }};
    {{end}}
};

{{obj.name}} *{{obj.name}}S::asset()
{
    {{obj.name}} *obj = new {{obj.name}}();
    {{ for var in obj.vars }}
    obj->{{ var.name }} = {{ var.name }};
    {{end}}

    return obj;
};

DatSize {{obj.name}}S::save()
{
    size_t size;
    {{ for var in obj.vars }}
    size += {{ var.size }};
    {{end}}
    START_SAVE()
    {{ for var in obj.vars }}
    S_{{var.type | string.upcase | string.replace ""::"" ""_"" | string.replace "" "" ""_""}}({{ var.name }});
    {{end}}
    END_SAVE()
};

void {{obj.name}}S::load(unsigned char *data, size_t size)
{
    START_LOAD()
    {{ for var in obj.vars }}
    L_{{var.type | string.upcase | string.replace ""::"" ""_"" | string.replace "" "" ""_""}}({{ var.name }});
    {{end}}
    END_LOAD()

};

const unsigned char {{obj.name}}S::getUUID()
{
    return {{obj.uuid_string}};
};

{{- end }}
}

/*
 * End of File
 */            
");
                int negaterLength = path.Count(x => x == '/');
                string negater = "";
                for(int i = 0;i < negaterLength;i++)
                {
                    negater = negater + "../";
                }

                var header_o = header.Render(new { objects = objs, file = file, includeFile = Path.GetFullPath(dir + negater + file.path + "/" + file.name) });

                var source_o = source.Render(new { include_name = Path.GetFileNameWithoutExtension(file.name) + ".generated.hpp", objects = objs });

                var header_p = Path.GetFullPath(dir + Path.GetFileNameWithoutExtension(file.name) + ".generated.hpp");

                var source_p = Path.GetFullPath(dir + Path.GetFileNameWithoutExtension(file.name) + ".generated.cpp");

                Directory.CreateDirectory(dir);
                if (!File.Exists(header_p) || (File.ReadAllText(header_p) != header_o))
                {
                    File.WriteAllText(header_p, header_o);
                }

                if (!File.Exists(source_p) || (File.ReadAllText(source_p) != source_o))
                {
                    File.WriteAllText(source_p, source_o);
                }

            }

            var registry = Template.Parse(@"
#include <iomanip>

#include ""Assets/AssetManager.hpp""
{{ for i in includes }}
#include ""{{ i }}""
{{- end }}
using namespace ckg;

namespace ck
{
AssetS *AssetManager::getObject(unsigned char UUID)
{
    {{for obj in objs}}
    {{obj.name}}S {{obj.name}};
    {{- end }}
    {{for obj in objs}}
    if (UUID == {{ obj.name }}.getUUID())
    {
        return new {{ obj.name }}S();
    }
    else
    {{- end }}
    {
        std::stringstream ss;
        ss << std::hex << std::setfill('0') << std::setw(4) << static_cast<int>(UUID);
        LOG(ERROR) << ""Could not find asset of UUID: 0x"" << ss.str();
        return nullptr;
    }
};
}
            ");
            var registry_o = registry.Render(new { objs = totalObjs, includes = totalIncludes });
            Directory.CreateDirectory(dir);
            var registry_p = Path.GetFullPath(dir + "Registry.generated.cpp");
            if (!File.Exists(registry_p) || (File.ReadAllText(registry_p) != registry_o))
            {
                File.WriteAllText(registry_p, registry_o);
            }
        }
    }
}
