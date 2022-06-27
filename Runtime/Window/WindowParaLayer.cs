using System.Collections.Generic;
using UnityEngine;

namespace eggsgd.UiFramework.Window
{
    /// <summary>
    ///     This is a "helper" layer so Windows with higher priority can be displayed.
    ///     By default, it contains any window tagged as a Popup. It is controlled by the WindowUILayer.
    /// </summary>
    public class WindowParaLayer : MonoBehaviour
    {
        [SerializeField]
        private GameObject darkenBgObject;

        private readonly List<GameObject> _containedScreens = new();

        public void AddScreen(Transform screenRectTransform)
        {
            screenRectTransform.SetParent(transform, false);
            _containedScreens.Add(screenRectTransform.gameObject);
        }

        public void RefreshDarken()
        {
            foreach (var t in _containedScreens)
            {
                if (t == null)
                {
                    continue;
                }

                if (!t.activeSelf)
                {
                    continue;
                }

                darkenBgObject.SetActive(true);
                return;
            }

            darkenBgObject.SetActive(false);
        }

        public void DarkenBg()
        {
            darkenBgObject.SetActive(true);
            darkenBgObject.transform.SetAsLastSibling();
        }
    }
}