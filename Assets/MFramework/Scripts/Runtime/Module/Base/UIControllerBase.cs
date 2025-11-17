using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Runtime
{
    public abstract class UIControllerBase<TView, TModel> : IUIController
          where TView : class, IUIView
          where TModel : IUIModel, new()
    {
        public TView View { get; private set; }
        public TModel Model { get; private set; }

        IUIView IUIController.View => View;
        IUIModel IUIController.Model => Model;

        public virtual void Initialize(IUIView view, IUIModel model)
        {
            View = view as TView;
            Model = (TModel)model;

            if (Model != null)
            {
                Model.OnDataChanged += OnModelDataChanged;
                Model.Initialize();
            }

            OnInitialized();
        }

        public abstract void OnShow(object data);
        public abstract void OnHide();
        public abstract void OnClose();

        protected virtual void OnInitialized() { }
        protected virtual void OnModelDataChanged(string propertyName) { }

        protected virtual void Dispose()
        {
            if (Model != null)
            {
                Model.OnDataChanged -= OnModelDataChanged;
            }
        }
    }
}
