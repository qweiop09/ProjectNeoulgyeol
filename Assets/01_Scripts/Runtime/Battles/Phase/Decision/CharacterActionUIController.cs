using System;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Decision
{
public class CharacterActionUIController : MonoBehaviour
{
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private RectTransform actionSettingPanel;
    [SerializeField] private Canvas actionSettingCanvas;
    [SerializeField] private Vector2 menuScreenOffset = new Vector2(120f, 0f);
    
    // 행동대상을 선택해야 할 때 외부에 알리는 이벤트
    public event Action<CharacterHandler> CompletedActionSetting;
    private CharacterHandler _currentHandler; // 행동을 설정 중인 캐릭터 핸들러
    
    private RectTransform _activeActionSettingMenu;
    // private ActData _currentActData; // 만들어지는 ActData

    private void Awake()
    {
        if (actionSettingPanel != null)
        {
            _activeActionSettingMenu = actionSettingPanel;
            _activeActionSettingMenu.gameObject.SetActive(false);

            if (actionSettingCanvas == null)
                actionSettingCanvas = actionSettingPanel.GetComponentInParent<Canvas>();
        }
    }

    public void HandleCharacterSelected(CharacterHandler characterHandler)
    {
        Debug.Log("setting action for character: "
                  + characterHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.name);
        _currentHandler = characterHandler;
        
        ShowActionMenu(characterHandler);
    }

    public void HandleSelectionCleared()
    {
        _currentHandler = null;
        
        HideActionMenu();
    }
    
    public void HideMenu()
    {
        HideActionMenu();
    }
    

    private void ShowActionMenu(CharacterHandler actData)
    {
        Debug.Log("ShowActionMenu called for character: "
                  + actData.characterBattleData.TargetingData[0].CastPlayerCharacter.name);
        
        RectTransform menu = GetActionMenu();
        if (menu == null) return;
        
        Vector3 menuScreenPosition = actData.characterBattleData.TargetingData[0].CastPlayerCharacter.transform.position + (Vector3)menuScreenOffset;
        
        GetActionMenu().gameObject.SetActive(true);

        menu.position = menuScreenPosition;
    }

    private void HideActionMenu()
    {
        Debug.Log("HideActionMenu called");
        
        if (_activeActionSettingMenu != null)
            _activeActionSettingMenu.gameObject.SetActive(false);
    }

    private RectTransform GetActionMenu()
    {
        if (_activeActionSettingMenu == null)
            return null;

        return _activeActionSettingMenu;
    }
    
    // 버튼 클릭
    
    public void PressedAttackButton(int useAttackNumber)
    {
        Debug.Log("Attack Button Pressed");

        Debug.Log(_currentHandler == null);
        Debug.Log(_currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter == null);
        Debug.Log(_currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData() == null);
        Debug.Log(_currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().TargetingData[0] == null);
        Debug.Log(_currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().CharacterData == null);
        Debug.Log(_currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().CharacterData.characterAttacks[0] == null);
        
         _currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().TargetingData[0].UseSkill = 
            _currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().CharacterData.characterAttacks[useAttackNumber];
         
        CompletedActionSetting?.Invoke(_currentHandler);
    }
    
    public void PressedSkillButton(int useSkillNumber)
    {
        Debug.Log("Skill Button Pressed");
        
        _currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().TargetingData[0].UseSkill = 
            _currentHandler.characterBattleData.TargetingData[0].CastPlayerCharacter.GetCharacterBattleData().CharacterData.characterSkills[useSkillNumber];
        
        CompletedActionSetting?.Invoke(_currentHandler);
    }
    
    // 아래에 UI 버튼 클릭 시 호출할 메서드 추가 (예: PressedDefendButton, PressedItemButton 등)
    
}
}
