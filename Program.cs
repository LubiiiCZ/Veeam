//Author: Luboš Dohnal
//Date: 2025-04-03
//As a hiring test task for Veeam

namespace Veeam;

class Program
{
    private static DirectorySyncer? _directorySyncer;

    public static void Main(string[] args)
    {
        // Check if the correct number of arguments are provided
        // Could go with 4 or more and just ignore the rest, but for simplicity we will just check for 4
        if (args.Length != 4)
        {
            Console.WriteLine("Missing program arguments, follow: Program.exe sourcePath replicaPath syncInterval(sec) logPath");
            return;
        }

        // Storing the arguments in variables for later use
        // Checking the interval to be a positive integer
        // Directories are checked in the constructor of the DirectorySyncer class
        var sourcePath = args[0];
        var replicaPath = args[1];
        if (!int.TryParse(args[2], out var syncInterval) || syncInterval < 1)
        {
            Console.WriteLine("Invalid interval. Must be a positive integer value (in seconds).");
            return;
        }
        var logPath = args[3];

        Console.WriteLine("Press any key to stop...");

        // Creating the DirectorySyncer object and starting the sync process
        // The constructor will check if the directories exist and create them if they don't
        _directorySyncer = new DirectorySyncer(sourcePath, replicaPath, syncInterval, logPath);
        _directorySyncer.StartSync();

        Console.ReadKey();
    }
}
