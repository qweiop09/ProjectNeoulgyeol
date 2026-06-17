using System;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Decision.ActionMenu
{
public class CharacterActionUIController : MonoBehaviour
{
    [SerializeField] private RectTransform actionSettingPanel;
    [SerializeField] private Canvas actionSettingCanvas;
    [SerializeField] private Vector2 menuScreenOffset = new Vector2(120f, 0f);

    [SerializeField] private CharacterActionMenuHandler actionMenuHandler;

    // CompletedActionSetting을 CharacterActionMenuHandler에서 받아서 외부로 전달
    public event Action<CharacterSkill> CompletedActionSetting;

    private void Awake()
    {
        if (actionSettingCanvas == null && actionSettingPanel != null)
            actionSettingCanvas = actionSettingPanel.GetComponentInParent<Canvas>();

        // CharacterActionMenuHandler의 이벤트를 외부로 중계
        if (actionMenuHandler != null)
            actionMenuHandler.CompletedActionSetting += skill => CompletedActionSetting?.Invoke(skill);
    }

    public void HandleCharacterSelected(CharacterHandler caster)
    {
        Debug.Log("setting action for character: "
                  + caster.characterBattleData.CharacterData.name);

        if (actionSettingPanel != null)
            actionSettingPanel.position =
                caster.transform.position + (Vector3)menuScreenOffset + new Vector3(0, 0, 0.1f);

        actionMenuHandler.ShowMenu(caster);
    }

    public void HandleSelectionCleared()
    {
        actionMenuHandler.HideMenu();
    }

    public void HideMenu()
    {
        actionMenuHandler.HideMenu();
    }
}
}