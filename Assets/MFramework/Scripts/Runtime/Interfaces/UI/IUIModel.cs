using System;
using System.Threading.Tasks;

namespace MFramework.Runtime
{
    public interface IUIModel
    {
        Task Init();
        void Reset();
    }
}
