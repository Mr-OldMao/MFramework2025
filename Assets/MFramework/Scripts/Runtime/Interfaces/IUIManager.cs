using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIManager : IGameModule , IUpdatableModule
{
    void ShowPanel<T>() where T : class;// TODO : IView;

    void HidePanel<T>() where T : class;// TODO : IView;
}
