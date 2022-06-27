using System;
using System.Collections.Generic;
using eggsgd.UiFramework.Core;
using UnityEngine;

namespace eggsgd.UiFramework.Window
{
    /// <summary>
    ///     This layer controls all Windows.
    ///     Windows are Screens that follow a history and a queue, and are displayed
    ///     one at a time (and may or may not be modals). This also includes pop-ups.
    /// </summary>
    public class WindowUILayer : AUiLayer<IWindowController>
    {
        [SerializeField] private WindowParaLayer priorityParaLayer;

        private HashSet<IUIScreenController> _screensTransitioning;
        private Stack<WindowHistoryEntry> _windowHistory;

        private Queue<WindowHistoryEntry> _windowQueue;

        public IWindowController CurrentWindow { get; private set; }

        private bool IsScreenTransitionInProgress => _screensTransitioning.Count != 0;

        public event Action RequestScreenBlock;
        public event Action RequestScreenUnblock;

        public override void Initialize()
        {
            base.Initialize();
            RegisteredScreens = new Dictionary<string, IWindowController>();
            _windowQueue = new Queue<WindowHistoryEntry>();
            _windowHistory = new Stack<WindowHistoryEntry>();
            _screensTransitioning = new HashSet<IUIScreenController>();
        }

        protected override void ProcessScreenRegister(string screenId, IWindowController controller)
        {
            base.ProcessScreenRegister(screenId, controller);
            controller.InTransitionFinished += OnInAnimationFinished;
            controller.OutTransitionFinished += OnOutAnimationFinished;
            controller.CloseRequest += OnCloseRequestedByWindow;
        }

        protected override void ProcessScreenUnregister(string screenId, IWindowController controller)
        {
            base.ProcessScreenUnregister(screenId, controller);
            controller.InTransitionFinished -= OnInAnimationFinished;
            controller.OutTransitionFinished -= OnOutAnimationFinished;
            controller.CloseRequest -= OnCloseRequestedByWindow;
        }

        public override void ShowScreen(IWindowController screen)
        {
            ShowScreen<IWindowProperties>(screen, null);
        }

        public override void ShowScreen<TProp>(IWindowController screen, TProp properties)
        {
            var windowProp = properties as IWindowProperties;

            if (ShouldEnqueue(screen, windowProp))
            {
                EnqueueWindow(screen, properties);
            }
            else
            {
                DoShow(screen, windowProp);
            }
        }

        public override void HideScreen(IWindowController screen)
        {
            if (screen == CurrentWindow)
            {
                _windowHistory.Pop();
                AddTransition(screen);
                screen.Hide();

                CurrentWindow = null;

                if (_windowQueue.Count > 0)
                {
                    ShowNextInQueue();
                }
                else if (_windowHistory.Count > 0)
                {
                    ShowPreviousInHistory();
                }
            }
            else
            {
                Debug.LogError(
                    $"[WindowUILayer] Hide requested on WindowId {screen.ScreenId} but that's not the currently open one ({(CurrentWindow != null ? CurrentWindow.ScreenId : "current is null")})! Ignoring request.");
            }
        }

        public override void HideAll(bool shouldAnimateWhenHiding = true)
        {
            base.HideAll(shouldAnimateWhenHiding);
            CurrentWindow = null;
            priorityParaLayer.RefreshDarken();
            _windowHistory.Clear();
        }

        public override void ReparentScreen(IUIScreenController controller, Transform screenTransform)
        {
            var window = controller as IWindowController;

            if (window == null)
            {
                Debug.LogError("[WindowUILayer] Screen " + screenTransform.name + " is not a Window!");
            }
            else
            {
                if (window.IsPopup)
                {
                    priorityParaLayer.AddScreen(screenTransform);
                    return;
                }
            }

            base.ReparentScreen(controller, screenTransform);
        }

        private void EnqueueWindow<TProp>(IWindowController screen, TProp properties) where TProp : IScreenProperties
        {
            _windowQueue.Enqueue(new WindowHistoryEntry(screen, (IWindowProperties)properties));
        }

        private bool ShouldEnqueue(IWindowController controller, IWindowProperties windowProp)
        {
            if (CurrentWindow == null && _windowQueue.Count == 0)
            {
                return false;
            }

            if (windowProp != null && windowProp.SuppressPrefabProperties)
            {
                return windowProp.WindowQueuePriority != WindowPriority.ForceForeground;
            }

            if (controller.WindowPriority != WindowPriority.ForceForeground)
            {
                return true;
            }

            return false;
        }

        private void ShowPreviousInHistory()
        {
            if (_windowHistory.Count > 0)
            {
                var window = _windowHistory.Pop();
                DoShow(window);
            }
        }

        private void ShowNextInQueue()
        {
            if (_windowQueue.Count > 0)
            {
                var window = _windowQueue.Dequeue();
                DoShow(window);
            }
        }

        private void DoShow(IWindowController screen, IWindowProperties properties)
        {
            DoShow(new WindowHistoryEntry(screen, properties));
        }

        private void DoShow(WindowHistoryEntry windowEntry)
        {
            if (CurrentWindow == windowEntry.Screen)
            {
                Debug.LogWarning(
                    string.Format(
                        "[WindowUILayer] The requested WindowId ({0}) is already open! This will add a duplicate to the " +
                        "history and might cause inconsistent behaviour. It is recommended that if you need to open the same" +
                        "screen multiple times (eg: when implementing a warning message pop-up), it closes itself upon the player input" +
                        "that triggers the continuation of the flow."
                        , CurrentWindow.ScreenId));
            }
            else if (CurrentWindow != null
                  && CurrentWindow.HideOnForegroundLost
                  && !windowEntry.Screen.IsPopup)
            {
                CurrentWindow.Hide();
            }

            _windowHistory.Push(windowEntry);
            AddTransition(windowEntry.Screen);

            if (windowEntry.Screen.IsPopup)
            {
                priorityParaLayer.DarkenBg();
            }

            windowEntry.Show();

            CurrentWindow = windowEntry.Screen;
        }

        private void OnInAnimationFinished(IUIScreenController screen)
        {
            RemoveTransition(screen);
        }

        private void OnOutAnimationFinished(IUIScreenController screen)
        {
            RemoveTransition(screen);
            if (screen is IWindowController { IsPopup: true, })
            {
                priorityParaLayer.RefreshDarken();
            }
        }

        private void OnCloseRequestedByWindow(IUIScreenController screen)
        {
            HideScreen(screen as IWindowController);
        }

        private void AddTransition(IUIScreenController screen)
        {
            _screensTransitioning.Add(screen);
            RequestScreenBlock?.Invoke();
        }

        private void RemoveTransition(IUIScreenController screen)
        {
            _screensTransitioning.Remove(screen);
            if (!IsScreenTransitionInProgress)
            {
                RequestScreenUnblock?.Invoke();
            }
        }
    }
}