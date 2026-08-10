using System;
using _01_Scripts.Runtime.Worlds.Inventory;
using _01_Scripts.Runtime.Worlds.Loot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01_Scripts.Runtime.Worlds.UI
{
    public class LootUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private LootItemSlotUI _slotPrefab;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        private LootResult _currentLoot;
        private Action _onConfirmed;

        private void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirm);
            _panel.SetActive(false);
        }

        public void Show(LootResult loot, Action onConfirmed)
        {
            // 루트 오브젝트가 씬에 비활성화 상태로 남아있어도(에디터 설정 실수 등) 여기서 스스로 깨운다 —
            // 처음 활성화되는 거면 Awake()가 이 시점에 동기적으로 실행되어 _confirmButton 리스너도 같이 붙는다.
            gameObject.SetActive(true);

            _currentLoot = loot;
            _onConfirmed = onConfirmed;

            ClearSlots();

            if (!loot.HasLoot)
            {
                // 드랍 없으면 바로 확인 처리
                onConfirmed?.Invoke();
                return;
            }

            foreach (var drop in loot.Drops)
            {
                var slot = Instantiate(_slotPrefab, _slotContainer);
                slot.Setup(drop);
            }

            if (_titleText != null)
                _titleText.text = "획득한 아이템";

            _panel.SetActive(true);
        }

        private void OnConfirm()
        {
            if (_currentLoot != null && InventoryManager.Instance != null)
            {
                foreach (var drop in _currentLoot.Drops)
                {
                    InventoryResult result = InventoryManager.Instance.AddItem(drop.Item, drop.Quantity);
                    if (result == InventoryResult.CapacityExceeded)
                        NotificationManager.Instance.Show($"인벤토리 공간이 부족해 '{drop.Item.itemName}'을(를) 습득하지 못했습니다.");
                    else if (result != InventoryResult.Success)
                        Debug.LogWarning($"[LootUI] '{drop.Item.itemName}' x{drop.Quantity} 획득 실패: {result}");
                }
            }

            _panel.SetActive(false);
            _onConfirmed?.Invoke();
            _currentLoot = null;
        }

        private void ClearSlots()
        {
            foreach (Transform child in _slotContainer)
                if (child.gameObject != _slotPrefab.gameObject)
                    Destroy(child.gameObject);
        }
    }
}
