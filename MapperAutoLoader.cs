namespace MyBatis.NET.Mapper;

public static class MapperAutoLoader
{
    /// <summary>
    /// Automatically loads all XML mapper files in the specified directory (default: "Mappers").
    /// </summary>
    public static void AutoLoad(string directory = "Mappers")
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"Mapper directory '{directory}' does not exist. Skipping AutoLoad.");
            return;
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

        Console.WriteLine($"AutoLoad complete: {loadedCount} SQL statement(s) from {files.Length} mapper file(s).");
    }
}
