using System;

namespace MFramework.Runtime
{
    public interface IUIModel
    {
        void Initialize();
        void Reset();

        event Action<string> OnDataChanged;
    }
}
