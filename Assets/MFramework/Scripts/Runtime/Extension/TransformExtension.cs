using UnityEngine;

namespace MFramework.Runtime
{
    public static class TransformExtension
    {
        public static T Find<T>(this Transform transform, string name) where T : Component
        {
            T result = default;
            var goArr = transform.GetComponentsInChildren<T>(true);
            for (int i = 0; i < goArr.Length; i++)
            {
                if (goArr[i].name == name)
                {
                    if (result == null)
                    {
                        result = goArr[i] as T;
                    }
                    else
                    {
                        Debugger.LogError($"Find more than one , name : {name} ");
                    }
#if !UNITY_EDITOR
                    break; 
#endif
                }
            }
            return result;
        }
    }
}
