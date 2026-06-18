using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _01_Scripts.Interfacese;

namespace _01_Scripts.Runtime.Worlds
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private float _fadeOutDuration = 0.4f;
        [SerializeField] private float _fadeInDuration = 0.4f;

        private Image _overlay;

        protected override void Awake()
        {
            base.Awake();
            _overlay = BuildOverlay();
        }

        public void LoadScene(string sceneName, Action onMidpoint = null)
        {
            StartCoroutine(LoadRoutine(sceneName, onMidpoint));
        }

        private IEnumerator LoadRoutine(string sceneName, Action onMidpoint)
        {
            yield return Fade(0f, 1f, _fadeOutDuration);

            onMidpoint?.Invoke();

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                yield return null;

            op.allowSceneActivation = true;
            yield return null;

            yield return Fade(1f, 0f, _fadeInDuration);
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
            var canvas = new GameObject("SceneLoaderCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvas.transform.SetParent(transform);

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
