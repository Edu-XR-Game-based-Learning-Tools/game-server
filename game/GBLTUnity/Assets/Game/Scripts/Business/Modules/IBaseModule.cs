using Cysharp.Threading.Tasks;
using Zenject;

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
        UniTask Initialize();

        UniTask CreateView(string viewId, ViewName viewName, IViewCustomFactory viewContextFactory);

        void Refresh(IModuleContextModel model);

        void CustomRefresh(IModuleContextModel model, string comparer);

        void Remove();

        void AddSubView(IViewContext subview);
    }

    public interface IViewCustomFactory : IValidatable
    {
        UniTask<IViewContext> Create(IBaseModule module, string viewId, ViewName viewName);
    }
}