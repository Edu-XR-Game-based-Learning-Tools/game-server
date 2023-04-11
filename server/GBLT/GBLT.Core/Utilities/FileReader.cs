namespace Core.Utility;

public class FileReader
{
    public static string Load(string dir)
    {
        if (CheckFileExist(dir))
            return GetContent(dir);

        throw new FileNotFound(dir);
    }

    private static string GetContent(string dir)
    {
        byte[] data = LoadFile(dir);
        string content = System.Text.Encoding.UTF8.GetString(data);

        return content;
    }

    public static bool CheckFileExist(string filePath)
    {
        return File.Exists(filePath);
    }

    private static byte[] LoadFile(string absolutePath)
    {
        if (absolutePath == null || absolutePath.Length == 0)
            return null;

        if (File.Exists(absolutePath))
            return File.ReadAllBytes(absolutePath);

        throw new FileNotFound(absolutePath);
    }

    private class FileNotFound : Exception
    {
        public FileNotFound(string mess) : base(mess)
        {
        }
    }
}