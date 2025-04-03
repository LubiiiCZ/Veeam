using System.Security.Cryptography;

namespace Veeam;

public static class Util
{
    private static readonly MD5 _md5 = MD5.Create();

    public static void TryCreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating directory '{path}': {ex.Message}");
            return;
        }
    }

    public static void TryDeleteWholeDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting directory '{path}': {ex.Message}");
        }
    }

    private static bool FilesAreEqual(string file1, string file2)
    {
        try
        {
            using var stream1 = File.OpenRead(file1);
            using var stream2 = File.OpenRead(file2);
            return _md5.ComputeHash(stream1).SequenceEqual(_md5.ComputeHash(stream2));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while comparing files '{file1}' and '{file2}': {ex.Message}");
            return false;
        }
    }

    public static void TryCopyFile(string sourceFile, string replicaFile)
    {
        if (!File.Exists(replicaFile) || !FilesAreEqual(sourceFile, replicaFile))
        {
            try
            {
                File.Copy(sourceFile, replicaFile, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while copying file from '{sourceFile}' to '{replicaFile}': {ex.Message}");
            }
        }
    }

    public static void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting file '{path}': {ex.Message}");
        }
    }
}
