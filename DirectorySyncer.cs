using System.Security.Cryptography;

namespace Veeam;

public class DirectorySyncer
{
    private readonly MD5 _md5 = MD5.Create();
    private readonly string _sourcePath;
    private readonly string _replicaPath;
    private readonly string _logPath;
    private readonly int _syncInterval;
    private Timer? _timer;
    private StreamWriter? _logWriter;

    public DirectorySyncer(string sourcePath, string replicaPath, int syncInterval, string logPath)
    {
        _sourcePath = sourcePath;
        _replicaPath = replicaPath;
        _syncInterval = syncInterval;
        _logPath = logPath;

        VerifyDirectories();
    }

    /// <summary>
    /// Verifies if the source and replica directories exist. If the replica or log directories do not exist, they are created.
    /// </summary>
    private void VerifyDirectories()
    {
        if (!Directory.Exists(_sourcePath))
        {
            Log($"Source path '{_sourcePath}' does not exist.");
            Environment.Exit(1);
        }

        if (!Directory.Exists(_replicaPath))
        {
            TryCreateDirectory(_replicaPath, true);
        }

        if (!Directory.Exists(_logPath))
        {
            TryCreateDirectory(_logPath, true);
        }
    }


    /// <summary>
    /// Starts the synchronization process. The directories are synchronized every syncInterval seconds.
    /// </summary>
    public void StartSync()
    {
        _timer = new Timer(SyncDirectories, null, 0, _syncInterval * 1000);
    }

    /// <summary>
    /// Synchronizes the source and replica directories.
    /// Each run is logged to a separate file in the log directory.
    /// </summary>
    private void SyncDirectories(object? state)
    {
        var logFilePath = Path.Combine(_logPath, $"sync_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        using (_logWriter = new StreamWriter(logFilePath, true) { AutoFlush = true })
        {
            try
            {
                Log($"Syncing started");
                CreateDirectories();
                DeleteDirectories();
                CopyFiles();
                DeleteFiles();
                Log($"Syncing finished");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Creates directories in the replica path that exist in the source path but not in the replica path.
    /// </summary>
    private void CreateDirectories()
    {
        var sourceDirs = Directory.GetDirectories(_sourcePath, "*", SearchOption.AllDirectories);

        foreach (var dir in sourceDirs)
        {
            var replicaDir = dir.Replace(_sourcePath, _replicaPath);
            if (Directory.Exists(replicaDir)) continue;
            TryCreateDirectory(replicaDir);
        }
    }

    /// <summary>
    /// Deletes directories in the replica path that do not exist in the source path.
    /// Deletes also everything inside those directories.
    /// </summary>
    private void DeleteDirectories()
    {
        var replicaDirs = Directory.GetDirectories(_replicaPath, "*", SearchOption.AllDirectories);

        foreach (var dir in replicaDirs)
        {
            if (!Directory.Exists(dir)) continue; //Could have been deleted as part of the sync process

            var sourceDir = dir.Replace(_replicaPath, _sourcePath);
            if (Directory.Exists(sourceDir)) continue; //Source directory exists, so we don't need to delete the replica

            TryDeleteWholeDirectory(dir);
        }
    }

    /// <summary>
    /// Copies files from the source path to the replica path.
    /// Only if the file does not exist in the replica path or if the files are different.
    /// </summary>
    private void CopyFiles()
    {
        var sourceFiles = Directory.GetFiles(_sourcePath, "*", SearchOption.AllDirectories);

        foreach (var file in sourceFiles)
        {
            var replicaFile = file.Replace(_sourcePath, _replicaPath);
            TryCopyFile(file, replicaFile);
        }
    }

    /// <summary>
    /// Deletes files in the replica path that do not exist in the source path.
    /// </summary>
    private void DeleteFiles()
    {
        var replicaFiles = Directory.GetFiles(_replicaPath, "*", SearchOption.AllDirectories);

        foreach (var file in replicaFiles)
        {
            var sourceFile = file.Replace(_replicaPath, _sourcePath);

            if (File.Exists(sourceFile)) continue; //Source file exists and the content is the same, so we don't need to delete the replica

            TryDeleteFile(file);
        }
    }

    /// <summary>
    /// Creates a directory if it does not exist.
    /// If the directory cannot be created, it logs the error and exits the program if exitOnError is true.
    /// </summary>
    private void TryCreateDirectory(string path, bool exitOnError = false)
    {
        try
        {
            Directory.CreateDirectory(path);
            Log($"Creating directory '{path}'");
        }
        catch (Exception ex)
        {
            Log($"An error occurred while creating directory '{path}': {ex.Message}");
            if (exitOnError) Environment.Exit(1);
        }
    }

    /// <summary>
    /// Deletes a directory and all its contents.
    /// </summary>
    private void TryDeleteWholeDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
            Log($"Deleting directory '{path}'");
        }
        catch (Exception ex)
        {
            Log($"An error occurred while deleting directory '{path}': {ex.Message}");
        }
    }

    /// <summary>
    /// Compares two files using MD5 hash to check if they are equal.
    /// </summary>
    private bool FilesAreEqual(string file1, string file2)
    {
        try
        {
            using var stream1 = File.OpenRead(file1);
            using var stream2 = File.OpenRead(file2);
            return _md5.ComputeHash(stream1).SequenceEqual(_md5.ComputeHash(stream2));
        }
        catch (Exception ex)
        {
            Log($"An error occurred while comparing files '{file1}' and '{file2}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Copies a file from source to replica if the file does not exist in the replica path or if the files are different.
    /// </summary>
    private void TryCopyFile(string sourceFile, string replicaFile)
    {
        if (!File.Exists(replicaFile) || !FilesAreEqual(sourceFile, replicaFile))
        {
            try
            {
                File.Copy(sourceFile, replicaFile, true);
                Log($"Copying file '{replicaFile}'");
            }
            catch (Exception ex)
            {
                Log($"An error occurred while copying file from '{sourceFile}' to '{replicaFile}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Deletes a file.
    /// Used when the file exists in the replica path and does not exist in the source path.
    /// </summary>
    private void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
            Log($"Deleting file '{path}'");
        }
        catch (Exception ex)
        {
            Log($"An error occurred while deleting file '{path}': {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a message to the console and to the log file.
    /// </summary>
    private void Log(string message)
    {
        string logMessage = $"[{DateTime.Now}] {message}";
        Console.WriteLine(logMessage);
        _logWriter?.WriteLine(logMessage);
    }
}
