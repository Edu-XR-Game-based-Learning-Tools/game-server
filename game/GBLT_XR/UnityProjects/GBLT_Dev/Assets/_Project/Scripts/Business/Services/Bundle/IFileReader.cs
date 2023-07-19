using Cysharp.Threading.Tasks;

namespace Core.Business
{
    public interface IFileReader
    {
        UniTask<T> Read<T>(string filePath);
    }
}