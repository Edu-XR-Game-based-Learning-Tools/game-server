using Core.Network;
using UnityEngine;

namespace Core.Framework
{
    [CreateAssetMenu(fileName = "InitialSetting", menuName = "Configs/InitialConfig", order = 1)]
    public class InitialSettingInstaller : ScriptableObject
    {
        public GameStore.Setting GameSetting;
        public GameStore.Atlas Atlas;
        public NetworkSettings NetworkSettings;
    }
}
