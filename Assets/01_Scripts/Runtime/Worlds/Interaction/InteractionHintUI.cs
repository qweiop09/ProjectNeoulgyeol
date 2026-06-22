using TMPro;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds.Interaction
{
    public class InteractionHintUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private GameObject _container;

        public void Show(string hint)
        {
            if (_container != null) _container.SetActive(true);
            if (_label != null) _label.text = $"[E]  {hint}";
        }

        public void Hide()
        {
            if (_container != null) _container.SetActive(false);
        }
    }
}
