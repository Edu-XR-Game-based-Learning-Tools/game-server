using Core.Business;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Framework
{
    public partial class GameStore
    {
        public class ModelState
        {
            public Dictionary<Type, IModuleContextModel> Models = new();

            public ModelState()
            {
                Models.Clear();
            }

            public bool HasModel<TModel>()
                where TModel : IModuleContextModel, new()
            {
                if (Models.ContainsKey(typeof(TModel)))
                    return true;
                return false;
            }

            public TModel GetModel<TModel>()
                where TModel : IModuleContextModel, new()
            {
                if (Models.TryGetValue(typeof(TModel), out IModuleContextModel instance))
                    return (TModel)instance;

                throw new MissingModel();
            }

            public TModel CreateNewModel<TModel>()
                where TModel : IModuleContextModel, new()
            {
                TModel model = new();
                Models.Add(model.GetType(), model);
                return model;
            }

            public void RemoveModel<T>() where T : IModuleContextModel
            {
                Type t = typeof(T);
                RemoveModelByType(t);
            }

            public void RemoveModel<T>(T model) where T : IModuleContextModel
            {
                Type t = model.GetType();
                RemoveModelByType(t);
            }

            private void RemoveModelByType(Type t)
            {
                if (Models.ContainsKey(t))
                {
                    Models[t].Module.Remove();
                    Models.Remove(t);
                }
            }

            public void RemoveAllModules()
            {
                var items = Models.Values.Select(d => d).ToList();
                foreach (var model in items)
                    RemoveModel(model);
            }

            public class MissingModel : Exception { }
        }
    }
}
