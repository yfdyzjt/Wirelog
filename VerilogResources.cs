using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Wirelog
{
    public static partial class Converter
    {
        private static readonly Dictionary<string, string> VFiles = [];
        private static string MainCpp = "";

        public static void LoadVModules()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            
            var vModulePrefix = $"{assembly.GetName().Name}.VModule.";

            foreach (var resourceName in resourceNames)
            {
                if (!resourceName.StartsWith(vModulePrefix)) continue;

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                var fileName = resourceName[vModulePrefix.Length..].Replace('_', '.');

                if (fileName.EndsWith(".v"))
                {
                    VFiles[fileName] = content;
                }
                else if (fileName == "main.cpp")
                {
                    MainCpp = content;
                }
            }
        }

        public static void WriteVModules(string targetDirectory)
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            foreach (var vFile in VFiles)
            {
                File.WriteAllText(Path.Combine(targetDirectory, vFile.Key), vFile.Value);
            }

            if (!string.IsNullOrEmpty(MainCpp))
            {
                File.WriteAllText(Path.Combine(targetDirectory, "main.cpp"), MainCpp);
            }
        }
    }
}