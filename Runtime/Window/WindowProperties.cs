﻿using System;
using eggsgd.UiFramework.Core;
using UnityEngine;

namespace eggsgd.UiFramework.Window
{
    /// <summary>
    ///     Properties common to all windows
    /// </summary>
    [Serializable]
    public class WindowProperties : IWindowProperties
    {
        [SerializeField]
        protected bool hideOnForegroundLost = true;

        [SerializeField]
        protected WindowPriority windowQueuePriority = WindowPriority.ForceForeground;

        [SerializeField]
        protected bool isPopup;

        public WindowProperties()
        {
            hideOnForegroundLost = true;
            windowQueuePriority = WindowPriority.ForceForeground;
            isPopup = false;
        }

        public WindowProperties(bool suppressPrefabProperties = false)
        {
            WindowQueuePriority = WindowPriority.ForceForeground;
            HideOnForegroundLost = false;
            SuppressPrefabProperties = suppressPrefabProperties;
        }

        public WindowProperties(WindowPriority priority, bool hideOnForegroundLost = false,
                                bool suppressPrefabProperties = false)
        {
            WindowQueuePriority = priority;
            HideOnForegroundLost = hideOnForegroundLost;
            SuppressPrefabProperties = suppressPrefabProperties;
        }

        /// <summary>
        ///     How should this window behave in case another window
        ///     is already opened?
        /// </summary>
        /// <value>
        ///     Force Foreground opens it immediately, Enqueue queues it so that it's opened as soon as
        ///     the current one is closed.
        /// </value>
        public WindowPriority WindowQueuePriority
        {
            get => windowQueuePriority;
            set => windowQueuePriority = value;
        }

        /// <summary>
        ///     Should this window be hidden when other window takes its foreground?
        /// </summary>
        /// <value><c>true</c> if hide on foreground lost; otherwise, <c>false</c>.</value>
        public bool HideOnForegroundLost
        {
            get => hideOnForegroundLost;
            set => hideOnForegroundLost = value;
        }

        /// <summary>
        ///     When properties are passed in the Open() call, should the ones
        ///     configured in the viewPrefab be overwritten?
        /// </summary>
        /// <value><c>true</c> if suppress viewPrefab properties; otherwise, <c>false</c>.</value>
        public bool SuppressPrefabProperties { get; set; }

        /// <summary>
        ///     Popups are displayed with a black background behind them and
        ///     in front of all other Windows
        /// </summary>
        /// <value><c>true</c> if this window is a popup; otherwise, <c>false</c>.</value>
        public bool IsPopup
        {
            get => isPopup;
            set => isPopup = value;
        }
    }
}