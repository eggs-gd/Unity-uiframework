using eggsgd.UiFramework.Core;

namespace eggsgd.UiFramework.Panel
{
    /// <summary>
    ///     Base class for panels that need no special Properties
    /// </summary>
    public abstract class APanelController : APanelController<PanelProperties>
    {
    }

    /// <summary>
    ///     Base class for Panels
    /// </summary>
    public abstract class APanelController<T> : AUiScreenController<T>, IPanelController where T : IPanelProperties
    {
        public PanelPriority Priority => Properties != null ? Properties.Priority : PanelPriority.None;

        protected sealed override void SetProperties(T props)
        {
            base.SetProperties(props);
        }
    }
}