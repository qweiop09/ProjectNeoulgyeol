using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _01_Scripts.Runtime.Worlds
{
    public class FadeTransition : RoomTransition
    {
        [SerializeField] private float _fadeInDuration = 0.4f;
        [SerializeField] private float _fadeOutDuration = 0.4f;

        private Image _overlay;

        private void Awake()
        {
            _overlay = BuildOverlay();
        }

        public override IEnumerator Execute(Action onMidpoint)
        {
            yield return Fade(0f, 1f, _fadeInDuration);
            onMidpoint?.Invoke();
            yield return Fade(1f, 0f, _fadeOutDuration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            Color color = _overlay.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(from, to, elapsed / duration);
                _overlay.color = color;
                yield return null;
            }

            color.a = to;
            _overlay.color = color;
        }

        private Image BuildOverlay()
        {
            var canvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            DontDestroyOnLoad(canvas.gameObject);

            canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("FadePanel").AddComponent<Image>();
            panel.transform.SetParent(canvas.transform, false);
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.sizeDelta = Vector2.zero;
            panel.color = new Color(0f, 0f, 0f, 0f);
            panel.raycastTarget = false;

            return panel;
        }
    }
}
