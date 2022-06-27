using eggsgd.UiFramework.Core;
using UnityEngine;

namespace eggsgd.UiFramework.Panel
{
    /// <summary>
    ///     This Layer controls Panels.
    ///     Panels are Screens that have no history or queuing,
    ///     they are simply shown and hidden in the Frame
    ///     eg: a HUD, an energy bar, a mini map etc.
    /// </summary>
    public class PanelUILayer : AUiLayer<IPanelController>
    {
        [SerializeField]
        [Tooltip(
            "Settings for the priority para-layers. A Panel registered to this layer will be reparented to a different para-layer object depending on its Priority.")]
        private PanelPriorityLayerList priorityLayers;

        public override void ReparentScreen(IUIScreenController controller, Transform screenTransform)
        {
            if (controller is IPanelController ctl)
            {
                ReparentToParaLayer(ctl.Priority, screenTransform);
            }
            else
            {
                base.ReparentScreen(controller, screenTransform);
            }
        }

        public override void ShowScreen(IPanelController screen)
        {
            screen.Show();
        }

        public override void ShowScreen<TProps>(IPanelController screen, TProps properties)
        {
            screen.Show(properties);
        }

        public override void HideScreen(IPanelController screen)
        {
            screen.Hide();
        }

        public bool IsPanelVisible(string panelId)
        {
            return RegisteredScreens.TryGetValue(panelId, out var panel) && panel.IsVisible;
        }

        private void ReparentToParaLayer(PanelPriority priority, Transform screenTransform)
        {
            if (!priorityLayers.ParaLayerLookup.TryGetValue(priority, out var trans))
            {
                trans = transform;
            }

            screenTransform.SetParent(trans, false);
        }
    }
}