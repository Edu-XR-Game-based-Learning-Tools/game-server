using Zenject;

namespace Core.Module
{
    public partial class DummyXR
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.BindInterfacesTo<DummyXR>().AsSingle();
            }
        }
    }
}