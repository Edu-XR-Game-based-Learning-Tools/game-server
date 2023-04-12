using Newtonsoft.Json;

namespace Core.Utility;

public class JsonFileReader
{
    public static T Load<T>(string dir)
    {
        string json = FileReader.Load(dir);
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error when parse [{0}][{1}]", dir, e);
            return default;
        }
    }
}