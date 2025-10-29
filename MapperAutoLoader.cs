using System.Reflection;
using System.Linq;

namespace MyBatis.NET.Mapper;

public static class MapperAutoLoader
{
    /// <summary>
    /// Automatically loads all XML mapper files in the specified directory (default: "Mappers").
    /// </summary>
    public static void AutoLoad(string directory = "Mappers")
    {
        AutoLoad(new[] { directory });
    }

    /// <summary>
    /// Automatically loads all XML mapper files in the specified directories.
    /// </summary>
    public static void AutoLoad(params string[] directories)
    {
        if (directories.Length == 0)
        {
            AutoLoad("Mappers");
            return;
        }

        int totalLoaded = 0;
        int totalFiles = 0;

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Mapper directory '{directory}' does not exist. Skipping.");
                continue;
            }

            var files = Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories);
            int loadedCount = 0;

            foreach (var file in files)
            {
                try
                {
                    var statements = XmlMapperLoader.LoadFromFile(file);
                    MapperRegistry.RegisterMany(statements);
                    loadedCount += statements.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load mapper '{file}': {ex.Message}");
                }
            }

            totalLoaded += loadedCount;
            totalFiles += files.Length;
            Console.WriteLine($"Loaded {loadedCount} statement(s) from {files.Length} file(s) in '{directory}'.");
        }

        Console.WriteLine($"AutoLoad complete: {totalLoaded} SQL statement(s) from {totalFiles} mapper file(s).");
    }

    /// <summary>
    /// Loads XML mapper files embedded as resources in the specified assemblies.
    /// </summary>
    public static void AutoLoadFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        int totalLoaded = 0;
        int totalResources = 0;

        foreach (var assembly in assemblies)
        {
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            int loadedCount = 0;

            foreach (var resourceName in resourceNames)
            {
                try
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        var statements = XmlMapperLoader.LoadFromStream(stream);
                        MapperRegistry.RegisterMany(statements);
                        loadedCount += statements.Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load embedded mapper '{resourceName}': {ex.Message}");
                }
            }

            totalLoaded += loadedCount;
            totalResources += resourceNames.Length;
            Console.WriteLine($"Loaded {loadedCount} statement(s) from {resourceNames.Length} embedded resource(s) in '{assembly.GetName().Name}'.");
        }

        Console.WriteLine($"AutoLoad from assemblies complete: {totalLoaded} SQL statement(s) from {totalResources} embedded resource(s).");
    }
}
