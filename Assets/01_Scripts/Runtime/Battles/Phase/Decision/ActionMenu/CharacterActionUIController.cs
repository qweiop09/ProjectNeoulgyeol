using System;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Decision.ActionMenu
{
public class CharacterActionUIController : MonoBehaviour
{
    [SerializeField] private RectTransform actionSettingPanel;
    [SerializeField] private Canvas actionSettingCanvas;
    [SerializeField] private Vector2 menuScreenOffset = new Vector2(120f, 0f);

    [SerializeField] private CharacterActionMenuHandler actionMenuHandler;

    public event Action<CharacterSkill> CompletedActionSetting;
    public event Action<Item> CompletedItemActionSetting;
    public event Action CompletedStayActionSetting;

    private void Awake()
    {
        if (actionSettingCanvas == null && actionSettingPanel != null)
            actionSettingCanvas = actionSettingPanel.GetComponentInParent<Canvas>();

        if (actionMenuHandler != null)
        {
            actionMenuHandler.CompletedActionSetting     += skill => CompletedActionSetting?.Invoke(skill);
            actionMenuHandler.CompletedItemActionSetting += item  => CompletedItemActionSetting?.Invoke(item);
            actionMenuHandler.CompletedStayActionSetting += ()   => CompletedStayActionSetting?.Invoke();
        }
    }

    public void HandleCharacterSelected(CharacterHandler caster)
    {
        Debug.Log("setting action for character: "
                  + caster.characterBattleData.CharacterData.characterName);

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
