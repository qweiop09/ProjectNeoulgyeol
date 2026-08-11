using UnityEditor;
using UnityEngine;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Worlds.Inventory;

[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 기존 categoryCapacities / debugTestItem / debugTestQuantity 필드 그대로

        var manager = (InventoryManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("디버그: 현재 인벤토리", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 인벤토리 내용을 확인/추가할 수 있습니다.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("디버그 아이템 넣기 (위 debugTestItem/debugTestQuantity 사용)"))
        {
            // 기존 ContextMenu("Debug: Add Test Item")와 동일한 동작 — private 메서드 대신 public API를 직접 호출
            SerializedProperty itemField = serializedObject.FindProperty("debugTestItem");
            SerializedProperty qtyField = serializedObject.FindProperty("debugTestQuantity");
            Item item = itemField.objectReferenceValue as Item;
            manager.AddItem(item, qtyField.intValue);
        }

        EditorGUILayout.Space(5);

        if (manager.Slots.Count == 0)
        {
            EditorGUILayout.LabelField("(비어있음)");
        }
        else
        {
            foreach (var slot in manager.Slots)
            {
                string label = slot.item != null
                    ? $"{slot.item.itemName}  x{slot.quantity}  [{slot.item.category}]"
                    : "(빈 슬롯)";
                if (slot.equippedBy != null)
                    label += $"  — 장착 중: {slot.equippedBy.name}";
                EditorGUILayout.LabelField(label);
            }
        }

        Repaint(); // 플레이 중 인벤토리가 바뀌면 바로바로 반영되도록 매 프레임 다시 그림
    }
}
