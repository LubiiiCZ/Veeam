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
        DeleteDirectories(_sourcePath, _replicaPath);
        CopyFiles(_sourcePath, _replicaPath);
        DeleteFiles(_sourcePath, _replicaPath);
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

    private static void DeleteDirectories(string source, string replica)
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

    private static void CopyFiles(string source, string replica)
    {
        var sourceFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

        foreach (var file in sourceFiles)
        {
            var replicaFile = file.Replace(source, replica);
            Util.TryCopyFile(file, replicaFile);
        }
    }

    private static void DeleteFiles(string source, string replica)
    {
        var replicaFiles = Directory.GetFiles(replica, "*", SearchOption.AllDirectories);

        foreach (var file in replicaFiles)
        {
            var sourceFile = file.Replace(replica, source);

            if (File.Exists(sourceFile)) continue; //Source file exists and the content is the same, so we don't need to delete the replica

            Util.TryDeleteFile(file);
        }
    }
}
