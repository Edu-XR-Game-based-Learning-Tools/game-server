using Cysharp.Threading.Tasks;

namespace Core.Business
{
    public interface IModuleContextModel
    {
        string ViewId { get; set; }

        IModuleContextModel Clone();

        IBaseModule Module { get; set; }

        void Refresh();

        void CustomRefresh(string comparer);
    }

    public interface IBaseModule
    {
        ModuleName ModuleName { get; }

        IViewContext ViewContext { get; }

        UniTask Initialize();

        UniTask CreateView(string viewId, ModuleName moduleName, IViewContext viewContext);

        void Refresh(IModuleContextModel model);

        void Remove();
    }
}
