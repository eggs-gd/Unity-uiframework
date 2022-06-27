using System;
using eggsgd.UiFramework.Core;
using eggsgd.UiFramework.Panel;
using eggsgd.UiFramework.Window;
using UnityEngine;
using UnityEngine.UI;

namespace eggsgd.UiFramework
{
    /// <summary>
    ///     This is the centralized access point for all things UI.
    ///     All your calls should be directed at this.
    /// </summary>
    public class UIFrame : MonoBehaviour
    {
        [Tooltip("Set this to false if you want to manually initialize this UI Frame.")]
        [SerializeField] private bool initializeOnAwake = true;

        private GraphicRaycaster _graphicRaycaster;

        private Canvas _mainCanvas;

        private PanelUILayer _panelLayer;
        private WindowUILayer _windowLayer;

        /// <summary>
        ///     The main canvas of this UI
        /// </summary>
        public Canvas MainCanvas
        {
            get
            {
                if (_mainCanvas == null)
                {
                    _mainCanvas = GetComponent<Canvas>();
                }

                return _mainCanvas;
            }
        }

        /// <summary>
        ///     The Camera being used by the Main UI Canvas
        /// </summary>
        public Camera UICamera => MainCanvas.worldCamera;

        private void Awake()
        {
            if (initializeOnAwake)
            {
                Initialize();
            }
        }

        /// <summary>
        ///     Initializes this UI Frame. Initialization consists of initializing both the Panel and Window layers.
        ///     Although literally all the cases I've had to this day were covered by the "Window and Panel" approach,
        ///     I made it virtual in case you ever need additional layers or other special initialization.
        /// </summary>
        public virtual void Initialize()
        {
            if (_panelLayer == null)
            {
                _panelLayer = gameObject.GetComponentInChildren<PanelUILayer>(true);
                if (_panelLayer == null)
                {
                    Debug.LogError("[UI Frame] UI Frame lacks Panel Layer!");
                }
                else
                {
                    _panelLayer.Initialize();
                }
            }

            if (_windowLayer == null)
            {
                _windowLayer = gameObject.GetComponentInChildren<WindowUILayer>(true);
                if (_panelLayer == null)
                {
                    Debug.LogError("[UI Frame] UI Frame lacks Window Layer!");
                }
                else
                {
                    _windowLayer.Initialize();
                    _windowLayer.RequestScreenBlock += OnRequestScreenBlock;
                    _windowLayer.RequestScreenUnblock += OnRequestScreenUnblock;
                }
            }

            _graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        ///     Shows a panel by its id, passing no Properties.
        /// </summary>
        /// <param name="screenId">Panel Id</param>
        public void ShowPanel(string screenId)
        {
            _panelLayer.ShowScreenById(screenId);
        }

        /// <summary>
        ///     Shows a panel by its id, passing parameters.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        /// <param name="properties">Properties.</param>
        /// <typeparam name="T">The type of properties to be passed in.</typeparam>
        /// <seealso cref="IPanelProperties" />
        public void ShowPanel<T>(string screenId, T properties) where T : IPanelProperties
        {
            _panelLayer.ShowScreenById(screenId, properties);
        }

        /// <summary>
        ///     Hides the panel with the given id.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void HidePanel(string screenId)
        {
            _panelLayer.HideScreenById(screenId);
        }

        /// <summary>
        ///     Opens the Window with the given Id, with no Properties.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void OpenWindow(string screenId)
        {
            _windowLayer.ShowScreenById(screenId);
        }

        /// <summary>
        ///     Closes the Window with the given Id.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void CloseWindow(string screenId)
        {
            _windowLayer.HideScreenById(screenId);
        }

        /// <summary>
        ///     Closes the currently open window, if any is open
        /// </summary>
        public void CloseCurrentWindow()
        {
            if (_windowLayer.CurrentWindow != null)
            {
                CloseWindow(_windowLayer.CurrentWindow.ScreenId);
            }
        }

        /// <summary>
        ///     Opens the Window with the given id, passing in Properties.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        /// <param name="properties">Properties.</param>
        /// <typeparam name="T">The type of properties to be passed in.</typeparam>
        /// <seealso cref="IWindowProperties" />
        public void OpenWindow<T>(string screenId, T properties) where T : IWindowProperties
        {
            _windowLayer.ShowScreenById(screenId, properties);
        }

        /// <summary>
        ///     Searches for the given id among the Layers, opens the Screen if it finds it
        /// </summary>
        /// <param name="screenId">The Screen id.</param>
        public void ShowScreen(string screenId)
        {
            Type type;
            if (IsScreenRegistered(screenId, out type))
            {
                if (type == typeof(IWindowController))
                {
                    OpenWindow(screenId);
                }
                else if (type == typeof(IPanelController))
                {
                    ShowPanel(screenId);
                }
            }
            else
            {
                Debug.LogError(string.Format("Tried to open Screen id {0} but it's not registered as Window or Panel!",
                    screenId));
            }
        }

        /// <summary>
        ///     Registers a screen. If transform is passed, the layer will
        ///     reparent it to itself. Screens can only be shown after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <param name="screenTransform">Screen transform. If not null, will be reparented to proper layer</param>
        public void RegisterScreen(string screenId, IUIScreenController controller, Transform screenTransform)
        {
            var window = controller as IWindowController;
            if (window != null)
            {
                _windowLayer.RegisterScreen(screenId, window);
                if (screenTransform != null)
                {
                    _windowLayer.ReparentScreen(controller, screenTransform);
                }

                return;
            }

            var panel = controller as IPanelController;
            if (panel != null)
            {
                _panelLayer.RegisterScreen(screenId, panel);
                if (screenTransform != null)
                {
                    _panelLayer.ReparentScreen(controller, screenTransform);
                }
            }
        }

        /// <summary>
        ///     Registers the panel. Panels can only be shown after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TPanel">The Controller type.</typeparam>
        public void RegisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController
        {
            _panelLayer.RegisterScreen(screenId, controller);
        }

        /// <summary>
        ///     Unregisters the panel.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TPanel">The Controller type.</typeparam>
        public void UnregisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController
        {
            _panelLayer.UnregisterScreen(screenId, controller);
        }

        /// <summary>
        ///     Registers the Window. Windows can only be opened after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TWindow">The Controller type.</typeparam>
        public void RegisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController
        {
            _windowLayer.RegisterScreen(screenId, controller);
        }

        /// <summary>
        ///     Unregisters the Window.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TWindow">The Controller type.</typeparam>
        public void UnregisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController
        {
            _windowLayer.UnregisterScreen(screenId, controller);
        }

        /// <summary>
        ///     Checks if a given Panel is open.
        /// </summary>
        /// <param name="panelId">Panel identifier.</param>
        public bool IsPanelOpen(string panelId) => _panelLayer.IsPanelVisible(panelId);

        /// <summary>
        /// Checks if a given Window is open.
        /// </summary>
        /// <param name="windowId">Window identifier.</param>
        public bool IsWindowOpen(string windowId)
        {
            if (string.IsNullOrEmpty(windowId))
            {
                return false;
            }

            return _windowLayer.CurrentWindow?.ScreenId == windowId;
        }

        /// <summary>
        ///     Hide all screens
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void HideAll(bool animate = true)
        {
            CloseAllWindows(animate);
            HideAllPanels(animate);
        }

        /// <summary>
        ///     Hide all screens on the Panel Layer
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void HideAllPanels(bool animate = true)
        {
            _panelLayer.HideAll(animate);
        }

        /// <summary>
        ///     Hide all screens in the Window Layer
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void CloseAllWindows(bool animate = true)
        {
            _windowLayer.HideAll(animate);
        }

        /// <summary>
        ///     Checks if a given screen id is registered to either the Window or Panel layers
        /// </summary>
        /// <param name="screenId">The Id to check.</param>
        public bool IsScreenRegistered(string screenId)
        {
            if (_windowLayer.IsScreenRegistered(screenId))
            {
                return true;
            }

            if (_panelLayer.IsScreenRegistered(screenId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks if a given screen id is registered to either the Window or Panel layers,
        ///     also returning the screen type
        /// </summary>
        /// <param name="screenId">The Id to check.</param>
        /// <param name="type">The type of the screen.</param>
        public bool IsScreenRegistered(string screenId, out Type type)
        {
            if (_windowLayer.IsScreenRegistered(screenId))
            {
                type = typeof(IWindowController);
                return true;
            }

            if (_panelLayer.IsScreenRegistered(screenId))
            {
                type = typeof(IPanelController);
                return true;
            }

            type = null;
            return false;
        }

        private void OnRequestScreenBlock()
        {
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }
        }

        private void OnRequestScreenUnblock()
        {
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = true;
            }
        }
    }
}