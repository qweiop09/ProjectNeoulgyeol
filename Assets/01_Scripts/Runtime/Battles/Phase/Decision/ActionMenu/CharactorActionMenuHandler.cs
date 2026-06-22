using System;
using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace _01_Scripts.Runtime.Battles.Phase.Decision.ActionMenu
{
public class CharacterActionMenuHandler : MonoBehaviour
{
    public enum ActionType
    {
        None,
        Attack,
        Skill,
        Item,
        Defend,
        Run
    }

    static String[] actionNames = new string[]
    {
        "Attack", "Skill", "Item", "Stay", "Run"
    };
    

    private CharacterHandler currentHandler; // 행동을 설정 중인 캐릭터 핸들러

    [SerializeField] private GameObject actionMenuPanel; // 행동 메뉴 패널
    [SerializeField] private TextMeshProUGUI actCharacterNameText; // 행동 메뉴에 표시될 캐릭터 이름 텍스트
    [SerializeField] private TextMeshProUGUI[] texts = new TextMeshProUGUI[5]; // 바뀔 텍스트들
    
    [SerializeField] private Button previousPageButton; // 이전 페이지 버튼
    [SerializeField] private Button nextPageButton; // 다음 페이지 버튼
    [SerializeField] private Button backButton; // 뒤로가기 버튼
    
    [SerializeField] private Slider hpSlider; // 체력 슬라이더
    [SerializeField] private Slider staminaSlider; // 스테미너 슬라이더

    [Header("internal")] private int actMenuPage = 0; // 현재 아이템을 어디서 부터 보여야 할지
    [SerializeField] private ActionType currentActionType = ActionType.None; // 현재 선택된 행동 유형


    public Action<CharacterSkill> CompletedActionSetting; // 스킬/공격 선택 완료 시 외부에 알리는 Action
    public Action<Item> CompletedItemActionSetting; // 아이템 선택 완료 시 외부에 알리는 Action
    public Action CompletedStayActionSetting; // Stay(대기) 선택 완료 시 외부에 알리는 Action

    private bool isActive = false; // 메뉴가 활성화되어 있는지 여부
    
    public void ShowMenu(CharacterHandler characterHandler)
    {
        currentHandler = characterHandler;
        isActive = true;
        
        UpdateMenu();

        // 행동 메뉴 패널 활성화
        actionMenuPanel.SetActive(true);
    }

    public void HideMenu()
    {
        currentHandler = null;
        currentActionType = ActionType.None;
        actMenuPage = 0;
        
        isActive = false;

        // 행동 메뉴 패널 비활성화
        actionMenuPanel.SetActive(false);
    }

    public void UpdateMenu()
    {
        UpdateActionMenuCharacterStatus();
        UpdateActionMenuTexts();

        if (currentActionType == ActionType.None)
        {
            previousPageButton.gameObject.SetActive(false);
            nextPageButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(false);
            
            return;
        }

        SetPageMoveButtons();
    }

    public void SetPageMoveButtons()
    {
        if (currentActionType == ActionType.Attack)
            SetPageMoveButtonsRefInt(currentHandler.GetCharacterBattleData().CharacterData.characterAttacks.Length);

        if (currentActionType == ActionType.Skill)
            SetPageMoveButtonsRefInt(currentHandler.GetCharacterBattleData().CharacterData.characterSkills.Length);

        if (currentActionType == ActionType.Item)
            SetPageMoveButtonsRefInt(currentHandler.GetCharacterBattleData().inventory.Count);
        
    }

    private void SetPageMoveButtonsRefInt(int arrayCount)
    {
        previousPageButton.gameObject.SetActive(actMenuPage != 0);
                        
        nextPageButton.gameObject.SetActive(actMenuPage != Mathf.CeilToInt(arrayCount / 5f) - 1);
        
        backButton.gameObject.SetActive(currentActionType != ActionType.None);
    }

    public void UpdateActionMenuCharacterStatus()
    {
        actCharacterNameText.text = currentHandler.GetCharacterBattleData().CharacterData.characterName;
        
        CharacterBattleData battleData = currentHandler.GetCharacterBattleData();
        
        hpSlider.value = battleData.currentHp / (float) battleData.CharacterData.maxHp * 100f;
        staminaSlider.value = battleData.currentStamina / (float) battleData.CharacterData.maxStamina * 100f;
    }

    public void UpdateActionMenuTexts()
    {
        if(isActive == false)
            return;
        
        if (currentActionType == ActionType.None)
            SetTexts(actionNames);

        if (currentActionType == ActionType.Attack)
            SetTexts(currentHandler.characterBattleData.CharacterData.characterAttacks);

        if (currentActionType == ActionType.Skill)
            SetTexts(currentHandler.characterBattleData.CharacterData.characterSkills);

        if (currentActionType == ActionType.Item)
            SetTexts(currentHandler.GetCharacterBattleData().inventory);

    }
    
    private void SetTexts(CharacterSkill[] skills)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i + actMenuPage * 5 < skills.Length)
            {
                texts[i].text = skills[i + actMenuPage * 5].skillName;
                texts[i].gameObject.SetActive(true);
            }
            else
            {
                texts[i].gameObject.SetActive(false);
                texts[i].text = "";
            }
        }
    }

    private void SetTexts(List<Item> items)
    {
        for (int i = 0; i < 5; i++)
        {
            int index = i + actMenuPage * 5;
            if (index < items.Count &&  items[index] != null)
            {
                texts[i].text = items[index].itemName;
                texts[i].gameObject.SetActive(true);
            }
            else
            {
                texts[i].gameObject.SetActive(false);
                texts[i].text = "";
            }
        }
    }

    private void SetTexts(String[] names)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i + actMenuPage * 5 < names.Length)
            {
                texts[i].text = names[i];
                texts[i].gameObject.SetActive(true);
            }
            else
            {
                texts[i].gameObject.SetActive(false);
                texts[i].text = "";
            }
        }
    }
    
    // UI랑 연결
    public void PressedButton(int pressedButtonNumber)
    {
        // 각 버튼의 눌림에 따른 함수 실행
        if (currentActionType == ActionType.None)
        {
            NoneStatePressed(pressedButtonNumber);
            return;
        }
        
        if (currentActionType == ActionType.Attack)
            AttackStatePressed(pressedButtonNumber);
        
        else if (currentActionType == ActionType.Skill)
            SkillStatePressed(pressedButtonNumber);
        
        else if (currentActionType == ActionType.Item)
            ItemStatePressed(pressedButtonNumber);
        
        else if(currentActionType == ActionType.Defend)
            CompletedActionSetting?.Invoke(null); // TODO: 방어 행동 설정 필요
        
        else if(currentActionType == ActionType.Run)
            CompletedActionSetting?.Invoke(null); // TODO: 도망 행동 설정 필요
        
        currentActionType = ActionType.None;

    }

    private void NoneStatePressed(int pressedButtonNumber)
    {
        switch (pressedButtonNumber)
        {
            case 0: // Attack
                currentActionType = ActionType.Attack;
                break;
            case 1: // Skill
                currentActionType = ActionType.Skill;
                break;
            case 2: // Item
                currentActionType = ActionType.Item;
                break;
            case 3: // Stay — 하위 메뉴 없이 즉시 완료
                CompletedStayActionSetting?.Invoke();
                return; // UpdateMenu 호출 없이 바로 종료
            case 4: // Run
                currentActionType = ActionType.Run;
                break;
        }
        
        UpdateMenu();
    }

    private void AttackStatePressed(int pressedButtonNumber)
    {
        if (pressedButtonNumber >= currentHandler.GetCharacterBattleData().CharacterData.characterAttacks.Length)
            return;
        
        CompletedActionSetting?.Invoke(currentHandler.GetCharacterBattleData().CharacterData
            .characterAttacks[pressedButtonNumber]);
        
        UpdateMenu(); 
    }

    private void SkillStatePressed(int pressedButtonNumber)
    {
        if (pressedButtonNumber >= currentHandler.GetCharacterBattleData().CharacterData.characterSkills.Length)
            return;
        
        CompletedActionSetting?.Invoke(currentHandler.GetCharacterBattleData().CharacterData
            .characterSkills[pressedButtonNumber]);
        
        UpdateMenu(); 
    }

    private void ItemStatePressed(int pressedButtonNumber)
    {
        List<Item> inventory = currentHandler.GetCharacterBattleData().inventory;
        int index = pressedButtonNumber + actMenuPage * 5;
        if (index >= inventory.Count) return;

        CompletedItemActionSetting?.Invoke(inventory[index]);
    }


    public void MovePage(int moveDirection)
    {
        actMenuPage += moveDirection;
        actMenuPage = Mathf.Clamp(actMenuPage, 0, GetMaxPage() - 1);
        UpdateMenu();
    }

    private int GetMaxPage()
    {
        return currentActionType switch
        {
            ActionType.Attack => Mathf.CeilToInt(
                currentHandler.GetCharacterBattleData().CharacterData.characterAttacks.Length / 5f),
            ActionType.Skill  => Mathf.CeilToInt(
                currentHandler.GetCharacterBattleData().CharacterData.characterSkills.Length / 5f),
            ActionType.Item   => Mathf.Max(1, Mathf.CeilToInt(
                currentHandler.GetCharacterBattleData().inventory.Count / 5f)),
            _                 => 1
        };
    }

    public void BackPage()
    {
        currentActionType = ActionType.None;
        actMenuPage = 0;
        
        UpdateMenu();
    }
}
}
