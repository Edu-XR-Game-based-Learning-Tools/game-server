using Cysharp.Threading.Tasks;
using System;

namespace Core.Business
{
    public interface IDataServiceController
    {
        UniTask CacheUserDatas();

        UniTask<byte[]> LoadDefinitions();

        UniTask<DateTime> GetServerTime();
    }
}
