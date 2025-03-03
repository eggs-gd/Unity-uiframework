﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using eggsgd.UiFramework.Panel;
using eggsgd.UiFramework.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace eggsgd.UiFramework.Editor
{
    public static class UIFrameworkTools
    {
        [MenuItem("Assets/Create/eGGs.gd UI/UI Frame in Scene", priority = 2)]
        public static void CreateUIFrameInScene()
        {
            CreateUIFrame();
        }

        [MenuItem("Assets/Create/eGGs.gd UI/UI Frame Prefab", priority = 1)]
        public static void CreateUIFramePrefab()
        {
            var frame = CreateUIFrame();

            var prefabPath = GetCurrentPath();
            prefabPath = EditorUtility.SaveFilePanel("UI Frame Prefab", prefabPath, "UIFrame", "prefab");

            if (prefabPath.StartsWith(Application.dataPath))
            {
                prefabPath = "Assets" + prefabPath.Substring(Application.dataPath.Length);
            }

            if (!string.IsNullOrEmpty(prefabPath))
            {
                CreateNewPrefab(frame, prefabPath);
            }

            Object.DestroyImmediate(frame);
        }

        private static GameObject CreateUIFrame()
        {
            var uiLayer = LayerMask.NameToLayer("UI");
            var root = new GameObject("UIFrame");
            var camera = new GameObject("UICamera");

            var cam = camera.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Depth;
            cam.cullingMask = LayerMask.GetMask("UI");
            cam.orthographic = true;
            cam.farClipPlane = 25;

            root.AddComponent<UIFrame>();
            var canvas = root.AddComponent<Canvas>();
            root.layer = uiLayer;

            // ScreenSpaceCamera allows you to have things like 3d models, particles
            // and post-fx rendering out of the box (shader/render order limitations still apply)
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;

            var transform = cam.transform;
            transform.SetParent(root.transform, false);
            transform.localPosition = new Vector3(0f, 0f, -1500f);

            var screenScaler = root.AddComponent<CanvasScaler>();
            screenScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            screenScaler.referenceResolution = new Vector2(1920, 1080);

            root.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.transform.SetParent(root.transform, false);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // Creating the layers
            var panelLayerGo = CreateRect("PanelLayer", root, uiLayer);
            var panelLayer = panelLayerGo.AddComponent<PanelUILayer>();

            var windowLayerGo = CreateRect("WindowLayer", root, uiLayer);
            var windowLayer = windowLayerGo.AddComponent<WindowUILayer>();

            var priorityPanelLayer = CreateRect("PriorityPanelLayer", root, uiLayer);

            var windowParaLayerGo = CreateRect("PriorityWindowLayer", root, uiLayer);
            var windowParaLayer = windowParaLayerGo.AddComponent<WindowParaLayer>();

            // setting the para layer via reflection
            SetPrivateField(windowLayer, windowParaLayer, "priorityParaLayer");

            var darkenGo = CreateRect("DarkenBG", windowParaLayer.gameObject, uiLayer);
            var darkenImage = darkenGo.AddComponent<Image>();
            darkenImage.color = new Color(0f, 0f, 0f, 0.75f);

            // setting the BG darkener via reflection
            SetPrivateField(windowParaLayer, darkenGo, "darkenBgObject");
            darkenGo.SetActive(false);

            var tutorialPanelLayer = CreateRect("TutorialPanelLayer", root, uiLayer);

            // Rigging all the Panel Para-Layers on the Panel Layer
            var prioritiesList = new List<PanelPriorityLayerListEntry>
            {
                new(PanelPriority.None, panelLayer.transform),
                new(PanelPriority.Prioritary, priorityPanelLayer.transform),
                new(PanelPriority.Tutorial, tutorialPanelLayer.transform),
            };
            var panelPriorities = new PanelPriorityLayerList(prioritiesList);

            SetPrivateField(panelLayer, panelPriorities, "priorityLayers");

            return root;
        }

        public static string GetCurrentPath()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            return path;
        }

        private static void SetPrivateField(object target, object element, string fieldName)
        {
            var prop = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic
              | BindingFlags.Instance);
            prop?.SetValue(target, element);
        }

        private static GameObject CreateRect(string name, GameObject parentGO, int layer)
        {
            var parent = parentGO.GetComponent<RectTransform>();
            var newRect = new GameObject(name, typeof(RectTransform))
            {
                layer = layer,
            };
            var rt = newRect.GetComponent<RectTransform>();

            rt.anchoredPosition = parent.position;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.transform.SetParent(parent, false);
            rt.sizeDelta = Vector3.zero;

            return newRect;
        }

        private static void CreateNewPrefab(GameObject obj, string localPath)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, localPath);
        }
    }
}