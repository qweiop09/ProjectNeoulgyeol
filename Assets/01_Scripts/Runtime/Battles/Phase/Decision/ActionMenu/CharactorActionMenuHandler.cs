using System;
using System.Net.Mime;
using _01_Scripts.DTO;
using NUnit.Framework.Internal.Filters;
using TMPro;
using Unity.VisualScripting;
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
        "Attack", "Skill", "Item", "Defend", "Run"
    };
    
    static int maxPage = 3; // TODO: 아이템 페이지 수에 따라 조정 필요

    private CharacterHandler currentHandler; // 행동을 설정 중인 캐릭터 핸들러

    [SerializeField] private GameObject actionMenuPanel; // 행동 메뉴 패널
    [SerializeField] private TextMeshProUGUI actCharacterNameText; // 행동 메뉴에 표시될 캐릭터 이름 텍스트
    [SerializeField] private TextMeshProUGUI[] texts = new TextMeshProUGUI[5]; // 바뀔 텍스트들
    
    [SerializeField] private Slider hpSlider; // 체력 슬라이더
    [SerializeField] private Slider staminaSlider; // 스테미너 슬라이더

    [Header("internal")] private int actMenuPage = 0; // 현재 아이템을 어디서 부터 보여야 할지
    [SerializeField] private ActionType currentActionType = ActionType.None; // 현재 선택된 행동 유형


    public Action<CharacterSkill> CompletedActionSetting; // 수행할 행동이 선택되었을 때 외부에 어떤 행동을 수행하는지 알리는 Action

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
        
        // 페이지 이동버튼 활성화 여부 업데이트
        // 이전 버튼 활성화 여부
    }

    public void UpdateActionMenuCharacterStatus()
    {
        actCharacterNameText.text = currentHandler.GetCharacterBattleData().CharacterData.name;
        
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

        if (currentActionType == ActionType.Item) { }
            // TODO: 아이템들 가져오는 기능 필요
            // SetTexts();

    }
    
    private void SetTexts(CharacterSkill[] skills)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (i < skills.Length)
            {
                texts[i].text = skills[i + actMenuPage * 5].skillName;
                texts[i].GameObject().SetActive(true);
            }
            else
            {
                texts[i].GameObject().SetActive(false);
                texts[i].text = "";
            }
        }
    }

    private void SetTexts(String[] names)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (i < names.Length)
            {
                texts[i].text = names[i];
                texts[i].GameObject().SetActive(true);
            }
            else
            {
                texts[i].GameObject().SetActive(false);
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
            case 3: // Defend
                currentActionType = ActionType.Defend;
                break;
            case 4: // Run
                currentActionType = ActionType.Run;
                break;
        }
        
        UpdateActionMenuTexts();
    }

    private void AttackStatePressed(int pressedButtonNumber)
    {
        if (pressedButtonNumber >= currentHandler.GetCharacterBattleData().CharacterData.characterAttacks.Length)
            return;
        
        CompletedActionSetting?.Invoke(currentHandler.GetCharacterBattleData().CharacterData
            .characterAttacks[pressedButtonNumber]);
    }

    private void SkillStatePressed(int pressedButtonNumber)
    {
        if (pressedButtonNumber >= currentHandler.GetCharacterBattleData().CharacterData.characterSkills.Length)
            return;
        
        CompletedActionSetting?.Invoke(currentHandler.GetCharacterBattleData().CharacterData
            .characterSkills[pressedButtonNumber]);
    }

    private void ItemStatePressed(int pressedButtonNumber)
    {
        // 기능
    }


    public void MovePage(int moveDirection)
    {
        actMenuPage += moveDirection;

        actMenuPage = Mathf.Clamp(actMenuPage, 0, maxPage - 1);
        // 아이템 불러와지면 클램프 범위 조정 필요
        
        UpdateActionMenuTexts();
    }

    public void BackPage()
    {
        currentActionType = ActionType.None;
        
        UpdateMenu();
    }
}
}
