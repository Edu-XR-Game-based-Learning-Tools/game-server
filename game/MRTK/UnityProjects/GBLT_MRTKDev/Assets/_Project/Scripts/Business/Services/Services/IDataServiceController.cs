using Cysharp.Threading.Tasks;
using Shared.Network;
using System;

namespace Core.Business
{
    public interface IDataServiceController
    {
        UniTask CacheUserDatas();

        UniTask<EnvironmentGenericConfig> GetGenericConfig();

        UniTask<byte[]> LoadDefinitions();

        UniTask<DateTime> GetServerTime();
    }
}
