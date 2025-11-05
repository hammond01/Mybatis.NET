using MyBatis.NET.Tools;

if (args.Length == 0)
{
    ShowHelp();
    return;
}

var command = args[0].ToLower();

switch (command)
{
    case "generate":
    case "gen":
        if (args.Length < 2)
        {
            Console.WriteLine("❌ Error: XML file path required");
            Console.WriteLine("Usage: dotnet run generate <xml-file-path> [output-path] [namespace]");
            return;
        }

        var xmlPath = args[1];
        var outputPath = args.Length > 2 ? args[2] : null;
        var namespaceOverride = args.Length > 3 ? args[3] : null;

        try
        {
            MapperInterfaceGenerator.GenerateAndSave(xmlPath, outputPath, namespaceOverride);
            Console.WriteLine("✅ Interface generated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        break;

    case "generate-all":
    case "gen-all":
        var searchDir = args.Length > 1 ? args[1] : "Mappers";
        var ns = args.Length > 2 ? args[2] : null;

        if (!Directory.Exists(searchDir))
        {
            Console.WriteLine($"❌ Error: Directory not found: {searchDir}");
            return;
        }

        var xmlFiles = Directory.GetFiles(searchDir, "*Mapper.xml", SearchOption.AllDirectories);
        Console.WriteLine($"Found {xmlFiles.Length} XML mapper file(s)");
        Console.WriteLine();

        foreach (var xmlFile in xmlFiles)
        {
            try
            {
                Console.Write($"Generating from {Path.GetFileName(xmlFile)}... ");
                MapperInterfaceGenerator.GenerateAndSave(xmlFile, null, ns);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"✅ Generated {xmlFiles.Length} interface(s)");
        break;

    case "help":
    case "-h":
    case "--help":
        ShowHelp();
        break;

    default:
        Console.WriteLine($"❌ Unknown command: {command}");
        Console.WriteLine();
        ShowHelp();
        break;
}

static void ShowHelp()
{
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  MyBatis.NET Mapper Interface Generator");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine();
    Console.WriteLine("USAGE:");
    Console.WriteLine("  dotnet run <command> [options]");
    Console.WriteLine();
    Console.WriteLine("COMMANDS:");
    Console.WriteLine("  generate, gen");
    Console.WriteLine("    Generate interface from a single XML mapper file");
    Console.WriteLine("    Usage: dotnet run generate <xml-file> [output-path] [namespace]");
    Console.WriteLine();
    Console.WriteLine("  generate-all, gen-all");
    Console.WriteLine("    Generate interfaces from all XML mappers in a directory");
    Console.WriteLine("    Usage: dotnet run generate-all [directory] [namespace]");
    Console.WriteLine();
    Console.WriteLine("  help, -h, --help");
    Console.WriteLine("    Show this help message");
    Console.WriteLine();
    Console.WriteLine("EXAMPLES:");
    Console.WriteLine("  # Generate from single file");
    Console.WriteLine("  dotnet run generate Mappers/UserMapper.xml");
    Console.WriteLine();
    Console.WriteLine("  # Generate with custom output path");
    Console.WriteLine("  dotnet run generate Mappers/UserMapper.xml Mappers/IUserMapper.cs");
    Console.WriteLine();
    Console.WriteLine("  # Generate with custom namespace");
    Console.WriteLine("  dotnet run generate Mappers/UserMapper.xml null MyApp.Data.Mappers");
    Console.WriteLine();
    Console.WriteLine("  # Generate all in directory");
    Console.WriteLine("  dotnet run generate-all Mappers");
    Console.WriteLine();
    Console.WriteLine("  # Generate all with custom namespace");
    Console.WriteLine("  dotnet run generate-all Mappers MyApp.Data.Mappers");
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
}
