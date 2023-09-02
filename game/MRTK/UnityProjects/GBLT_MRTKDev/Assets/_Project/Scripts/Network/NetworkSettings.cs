namespace Core.Network
{
    public enum HostingType
    {
        Local,
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
