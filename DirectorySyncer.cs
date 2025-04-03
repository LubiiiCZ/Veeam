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

    public void StartSync()
    {
        _timer = new Timer(SyncDirectories, null, 0, _syncInterval * 1000);
    }

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

    private void CopyFiles()
    {
        var sourceFiles = Directory.GetFiles(_sourcePath, "*", SearchOption.AllDirectories);

        foreach (var file in sourceFiles)
        {
            var replicaFile = file.Replace(_sourcePath, _replicaPath);
            TryCopyFile(file, replicaFile);
        }
    }

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

    private void Log(string message)
    {
        string logMessage = $"[{DateTime.Now}] {message}";
        Console.WriteLine(logMessage);
        _logWriter?.WriteLine(logMessage);
    }
}
