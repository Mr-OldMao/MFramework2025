using System;

namespace MFramework.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UIBindAttribute : Attribute
    {
        public Type ControllerType { get; }
        public Type ModelType { get; }

        public UIBindAttribute(Type uiController, Type uiModel)
        {
            ControllerType = uiController;
            ModelType = uiModel;

            // 只做类型验证，不创建实例
            ValidateTypes();
        }

        private void ValidateTypes()
        {
            // 验证Controller类型
            if (!typeof(UIControllerBase).IsAssignableFrom(ControllerType))
            {
                throw new ArgumentException($"UI控制类 {ControllerType.Name} 必须继承自 UIControllerBase");
            }

            // 验证Model类型
            if (!typeof(UIModelBase).IsAssignableFrom(ModelType))
            {
                throw new ArgumentException($"UI数据类 {ModelType.Name} 必须继承自 UIModelBase");
            }

            // 验证Model构造函数
            var constructors = ModelType.GetConstructors();
            bool hasValidConstructor = false;
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0 ||
                    (parameters.Length == 1 && parameters[0].ParameterType == typeof(IUIController)))
                {
                    hasValidConstructor = true;
                    break;
                }
            }

            if (!hasValidConstructor)
            {
                throw new ArgumentException($"UI数据类 {ModelType.Name} 必须有无参构造函数或接受IUIController参数的构造函数");
            }
        }
    }
}
