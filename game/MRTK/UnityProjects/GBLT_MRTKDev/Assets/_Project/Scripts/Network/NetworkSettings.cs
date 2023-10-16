namespace Core.Network
{
    public enum HostingType
    {
        Local,
        Local_IP,
        Develop,
        Production,
        Stage,
        Distributed
    }

    [System.Serializable]
    public struct NetworkSettings
    {
        public HostingType HostType;
        [System.Serializable]
        public class ThingWithArrays
        {
            public string[] Array;
        }

        public ThingWithArrays[] DefaultApiEndPoints;
    }
}
