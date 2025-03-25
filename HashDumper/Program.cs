using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;
using Yura.Shared.Archive;
using Yura.Shared.IO;

// Define all command line options
var command = new RootCommand("Scan a folder for archives and dump all unique hashes")
{
    new Argument<DirectoryInfo>(
        name: "path",
        description: "The path of the folder to scan"),

    new Argument<FileInfo>(
        name: "output",
        description: "The path of the output file"),

    new Option<Game>(
        aliases: ["-g", "--game"],
        description: "The game of the files",
        getDefaultValue: () => Game.Legend),

    new Option<Endianness>(
        aliases: ["-e", "--endianness"],
        description: "The endianness of the files",
        getDefaultValue: () => Endianness.LittleEndian),

    new Option<string>(
        aliases: ["-p", "--pattern"],
        description: "The pattern of the archives to scan",
        getDefaultValue: () => "*.000"),

    new Option<Format>(
        aliases: ["-f", "--format"],
        description: "The format to output the data in",
        getDefaultValue: () => Format.Text),
};

command.Handler = CommandHandler.Create(Execute);

// Run the command
await command.InvokeAsync(args);

static void Execute(
    DirectoryInfo path, FileInfo output, Game game, Endianness endianness, string pattern, Format format)
{
    List<ulong> hashes = [];

    foreach (var file in path.GetFiles(pattern))
    {
        Console.WriteLine(file.Name);

        // Open the archive
        var options = new ArchiveOptions
        {
            Path = file.FullName,
            Endianness = endianness
        };

        var archive = Create(game, options);
        archive.Open();

        // Add all hashes
        foreach (var record in archive.Records)
        {
            if (!hashes.Contains(record.Hash))
            {
                hashes.Add(record.Hash);
            }
        }
    }

    // Write the output
    switch (format)
    {
        case Format.Text:
            File.WriteAllLines(output.FullName, hashes.Select(hash => hash.ToString("X8")));

            break;
        case Format.Json:
            File.WriteAllText(output.FullName, JsonSerializer.Serialize(hashes));

            break;
    }
}

static ArchiveFile Create(Game game, ArchiveOptions options) => game switch
{
    Game.Defiance => new DefianceArchive(options),
    Game.Legend => new LegendArchive(options),
    Game.DeusEx => new DeusExArchive(options),
    Game.Tiger => new TigerArchive(options),

    _ => throw new NotImplementedException()
};

enum Game
{
    Defiance,
    Legend,
    DeusEx,
    Tiger
}

enum Format
{
    Text,
    Json
}