using System;
using eggsgd.UiFramework.Panel;
using eggsgd.UiFramework.Window;

namespace eggsgd.UiFramework.Core
{
    /// <summary>
    ///     Interface that all UI Screens must implement directly or indirectly
    /// </summary>
    public interface IUIScreenController
    {
        string ScreenId { get; set; }
        bool IsVisible { get; }

        Action<IUIScreenController> InTransitionFinished { get; set; }
        Action<IUIScreenController> OutTransitionFinished { get; set; }
        Action<IUIScreenController> CloseRequest { get; set; }
        Action<IUIScreenController> ScreenDestroyed { get; set; }

        void Show(IScreenProperties props = null);
        void Hide(bool animate = true);
    }

    /// <summary>
    ///     Interface that all Windows must implement
    /// </summary>
    public interface IWindowController : IUIScreenController
    {
        bool HideOnForegroundLost { get; }
        bool IsPopup { get; }
        WindowPriority WindowPriority { get; }
    }

    /// <summary>
    ///     Interface that all Panels must implement
    /// </summary>
    public interface IPanelController : IUIScreenController
    {
        PanelPriority Priority { get; }
    }
}