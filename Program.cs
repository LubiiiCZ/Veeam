namespace Veeam;

class Program
{
    private static string _sourcePath = string.Empty;
    private static string _replicaPath = string.Empty;
    private static string _logFilePath = string.Empty;
    private static int _syncInterval = 0;

    public static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Missing program arguments, follow: Program.exe sourcePath replicaPath syncInterval(sec) logPath");
            return;
        }

        _sourcePath = args[0];
        _replicaPath = args[1];
        if (!int.TryParse(args[2], out _syncInterval) || _syncInterval < 1)
        {
            Console.WriteLine("Invalid interval. Must be a positive integer value (in seconds).");
            return;
        }
        _logFilePath = args[3];
    }
}
