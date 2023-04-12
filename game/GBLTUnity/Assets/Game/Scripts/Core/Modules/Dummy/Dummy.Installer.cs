using Zenject;

namespace Core.Module
{
    public partial class Dummy
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.BindInterfacesTo<Dummy>().AsSingle();
            }
        }
    }
}