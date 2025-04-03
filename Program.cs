namespace Veeam;

class Program
{
    private static DirectorySyncer? _directorySyncer;

    public static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Missing program arguments, follow: Program.exe sourcePath replicaPath syncInterval(sec) logPath");
            return;
        }

        var sourcePath = args[0];
        var replicaPath = args[1];
        if (!int.TryParse(args[2], out var syncInterval) || syncInterval < 1)
        {
            Console.WriteLine("Invalid interval. Must be a positive integer value (in seconds).");
            return;
        }
        var logPath = args[3];

        _directorySyncer = new DirectorySyncer(sourcePath, replicaPath, syncInterval, logPath);
        _directorySyncer.StartSync();

        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();
    }
}
