using System;
using System.Threading.Tasks;
using static MFramework.Runtime.UIViewBase;

namespace MFramework.Runtime
{
    public abstract class UIControllerBase : IUIController
    {
        public IUIView View { get; private set; }
        public IUIModel Model { get; private set; }

        public UIStateProgressType StateProgress { get; private set; } = UIStateProgressType.Unstart;

        private Task m_TaskInit;

        public virtual async Task Initialize(IUIView view, IUIModel model)
        {
            View = view;
            Model = model;

            Model?.Initialize();
            m_TaskInit = View?.Initialize();
            await m_TaskInit;
            StateProgress = UIStateProgressType.InitCompleted;
        }


        public void SetStateProgress(UIStateProgressType stateProgress)
        {
            StateProgress = stateProgress;
        }

        protected virtual void Dispose()
        {

        }

        public virtual async Task Show(object showData = null, object showBeforeData = null)
        {
            await m_TaskInit;
            await OnShowBefore(showBeforeData);
            StateProgress = UIStateProgressType.ShowBeforeCompleted;
            View.UIForm.SetActive(true);
            OnShow(showData);
            StateProgress = UIStateProgressType.ShowCompleted;
        }

        public virtual async Task Hide(object hideData = null, object hideBoforeData = null)
        {
            await m_TaskInit;
            await OnHideBefore(hideBoforeData);
            StateProgress = UIStateProgressType.HideBeforeCompleted;
            View.UIForm.SetActive(false);
            OnHide(hideData);
            StateProgress = UIStateProgressType.HideCompleted;
        }

        public virtual void DestoryUI()
        {
            GameEntry.UI.DestroyView(View);
            UnityEngine.Object.Destroy(View.UIForm);
            StateProgress = UIStateProgressType.DestoryCompleted;
        }


        protected virtual void OnShow(object data) { }
        protected virtual Task OnShowBefore(object data) => Task.CompletedTask;
        protected virtual void OnHide(object data) { }
        protected virtual Task OnHideBefore(object data) => Task.CompletedTask;
    }
}
