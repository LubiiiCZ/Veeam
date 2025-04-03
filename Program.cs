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
        VerifyDirectories();
        CreateDirectories(_sourcePath, _replicaPath);
        RemoveDirectories(_sourcePath, _replicaPath);
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

    private static void VerifyDirectories()
    {
        if (!Directory.Exists(_sourcePath))
        {
            Console.WriteLine($"Source path '{_sourcePath}' does not exist.");
            return;
        }

        Util.TryCreateDirectory(_replicaPath);
        Util.TryCreateDirectory(_logPath);
    }

    private static void CreateDirectories(string source, string replica)
    {
        var sourceDirs = Directory.GetDirectories(source, "*", SearchOption.AllDirectories);

        foreach (var dir in sourceDirs)
        {
            var replicaDir = dir.Replace(source, replica);
            Util.TryCreateDirectory(replicaDir);
        }
    }

    private static void RemoveDirectories(string source, string replica)
    {
        var replicaDirs = Directory.GetDirectories(replica, "*", SearchOption.AllDirectories);

        foreach (var dir in replicaDirs)
        {
            if (!Directory.Exists(dir)) continue; //Could have been deleted as part of the sync process

            var sourceDir = dir.Replace(replica, source);
            if (Directory.Exists(sourceDir)) continue; //Source directory exists, so we don't need to delete the replica

            Util.TryDeleteWholeDirectory(dir);
        }
    }
}
