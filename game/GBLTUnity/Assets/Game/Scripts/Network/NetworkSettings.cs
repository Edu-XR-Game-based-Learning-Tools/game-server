namespace Core.Network
{
    public enum HostingType
    {
        Local,
        Develop,
        Stage,
        Distributed
    }

    public enum HostingEnvironment
    {
        Local = 0,
        Develop,
        Production,
        Stage
    }

    [System.Serializable]
    public struct NetworkSettings
    {
        public HostingType HostType;
        public HostingEnvironment HostEnv;
        public string BaseUrlServices;
        public string BaseUrlHubs;
    }
}