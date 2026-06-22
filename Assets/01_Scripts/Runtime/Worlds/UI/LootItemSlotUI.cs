using _01_Scripts.Runtime.Worlds.Loot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01_Scripts.Runtime.Worlds.UI
{
    public class LootItemSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _quantityText;

        public void Setup(LootDrop drop)
        {
            if (_icon != null)
            {
                _icon.sprite = drop.Item.icon;
                _icon.enabled = drop.Item.icon != null;
            }

            if (_nameText != null)
                _nameText.text = drop.Item.itemName;

            if (_quantityText != null)
                _quantityText.text = $"x{drop.Quantity}";
        }
    }
}
