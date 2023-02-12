using Core.Network;
using UnityEngine;
using Zenject;

namespace Core.Framework
{
    [CreateAssetMenu(fileName = "InitialSetting", menuName = "Configs/InitialConfig", order = 1)]
    public class InitialSettingInstaller : ScriptableObjectInstaller<InitialSettingInstaller>
    {
        public GameStore.Setting GameSetting;
        public GameStore.Atlas Atlas;
        public NetworkSettings NetworkSettings;

        public override void InstallBindings()
        {
            Container.BindInstance(GameSetting);
            Container.BindInstance(Atlas);
            Container.BindInstance(NetworkSettings);
        }
    }
}