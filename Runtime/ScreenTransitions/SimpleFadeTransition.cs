using System;
using UnityEngine;

namespace eggsgd.UiFramework.ScreenTransitions
{
    /// <summary>
    ///     This is a simple fade transition implemented as a built-in example.
    ///     I recommend using a free tweening library like DOTween (http://dotween.demigiant.com/)
    ///     or rolling out your own.
    ///     Check the Examples project for more robust and battle-tested options:
    ///     https://github.com/yankooliveira/uiframework_examples
    /// </summary>
    public class SimpleFadeTransition : ATransitionComponent
    {
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private bool fadeOut;

        private CanvasGroup _canvasGroup;
        private Action _currentAction;
        private Transform _currentTarget;
        private float _endValue;

        private bool _shouldAnimate;

        private float _startValue;
        private float _timer;

        private void Update()
        {
            if (!_shouldAnimate)
            {
                return;
            }

            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(_endValue, _startValue, _timer / fadeDuration);
            }
            else
            {
                _canvasGroup.alpha = 1f;
                _currentAction?.Invoke();

                _currentAction = null;
                _shouldAnimate = false;
            }
        }

        public override void Animate(Transform target, Action callWhenFinished)
        {
            if (_currentAction != null)
            {
                _canvasGroup.alpha = _endValue;
                _currentAction();
            }

            _canvasGroup = target.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            if (fadeOut)
            {
                _startValue = 1f;
                _endValue = 0f;
            }
            else
            {
                _startValue = 0f;
                _endValue = 1f;
            }

            _currentAction = callWhenFinished;
            _timer = fadeDuration;

            _canvasGroup.alpha = _startValue;
            _shouldAnimate = true;
        }
    }
}