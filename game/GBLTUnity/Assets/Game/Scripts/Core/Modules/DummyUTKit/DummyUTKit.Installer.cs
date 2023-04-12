using Zenject;

namespace Core.Module
{
    public partial class DummyUTKit
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.BindInterfacesTo<DummyUTKit>().AsSingle();
            }
        }
    }
}