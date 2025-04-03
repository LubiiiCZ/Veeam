namespace Veeam;

class Program
{
    private static string _sourcePath = string.Empty;
    private static string _replicaPath = string.Empty;
    private static string _logPath = string.Empty;
    private static int _syncInterval = 0;

    public static void Main(string[] args)
    {
        VerifyArguments(args);
        VerifyFolders();
    }

    private static void VerifyArguments(string[] args)
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
        _logPath = args[3];
    }

    private static void VerifyFolders()
    {
        if (!Directory.Exists(_sourcePath))
        {
            Console.WriteLine($"Source path '{_sourcePath}' does not exist.");
            return;
        }

        try
        {
            Directory.CreateDirectory(_replicaPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating replica folder '{_replicaPath}': {ex.Message}");
            return;
        }

        try
        {
            Directory.CreateDirectory(_logPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating log folder '{_logPath}': {ex.Message}");
            return;
        }
    }
}
