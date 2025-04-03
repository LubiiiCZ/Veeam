namespace Veeam;

public static class Util
{
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
}
