using System;
using System.Threading.Tasks;
using static MFramework.Runtime.UIViewBase;

namespace MFramework.Runtime
{
    public abstract class UIControllerBase : IUIController
    {
        public IUIView View { get; private set; }
        public IUIModel Model { get; private set; }

        private Task m_TaskInit;

        public virtual async Task Initialize(IUIView view, IUIModel model)
        {
            View = view;
            Model = model;

            Model?.Initialize();
            m_TaskInit = View?.Initialize();
            await m_TaskInit;
            GameEntry.UI.SetState(View, UIStateProgressType.InitCompleted);
        }

        protected virtual void Dispose()
        {

        }



        public virtual async Task Show(object showBeforeData = null, object showAfterData = null)
        {
            await m_TaskInit;
            await OnShow(showBeforeData);
            GameEntry.UI.SetState(View, UIStateProgressType.ShowBeforeCompleted);
            View.UIForm.SetActive(true);
            await OnShowAfter(showAfterData);
            GameEntry.UI.SetState(View, UIStateProgressType.ShowCompleted);
        }

        public virtual async Task Hide(object hideAfterData = null, object hideBoforeData = null)
        {
            await m_TaskInit;
            await OnHideBefore(hideBoforeData);
            GameEntry.UI.SetState(View, UIStateProgressType.HideBeforeCompleted);
            View.UIForm.SetActive(false);
            await OnHide(hideAfterData);
            GameEntry.UI.SetState(View, UIStateProgressType.HideCompleted);
        }

        public virtual void OnDestory()
        {
            GameEntry.UI.Clear(View);
            UnityEngine.Object.Destroy(View.UIForm);
            GameEntry.UI.SetState(View, UIStateProgressType.DestoryCompleted);
        }

        protected virtual Task OnShow(object data) => Task.CompletedTask;
        protected virtual Task OnShowAfter(object data) => Task.CompletedTask;
        protected virtual Task OnHide(object data) => Task.CompletedTask;
        protected virtual Task OnHideBefore(object data) => Task.CompletedTask;
    }
}
