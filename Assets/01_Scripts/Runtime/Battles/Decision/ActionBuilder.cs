using System;
using _01_Scripts.Runtime.Battles;
using _01_Scripts.Runtime.Battles.Decision;
using Unity.VisualScripting;
using UnityEngine;

public class ActionBuilder : MonoBehaviour
{
    
    public ActData BuildAction(CharacterHandler characterHandler, int useSlot, int useSkill
        , CharacterHandler targetPlayerCharacter, int targetSlot)
    {
        Debug.Log("Building action for character: " + characterHandler.name);
        
        
        Debug.Log(characterHandler);
        Debug.Log(characterHandler.GetCharacterBattleData());
        Debug.Log(characterHandler.GetCharacterBattleData().CharacterData);
        Debug.Log(characterHandler.GetCharacterBattleData().CharacterData.characterSkills);
        Debug.Log(characterHandler.GetCharacterBattleData().CharacterData.characterSkills[useSkill]);
        Debug.Log(characterHandler.GetCharacterBattleData().CharacterData.characterSkills[useSkill] == null);
        
        ActData returnActData = 
            new ActData(characterHandler, useSlot, 
                characterHandler.GetCharacterBattleData().CharacterData.characterSkills[useSkill],
                targetPlayerCharacter , targetSlot);
        
        // TODO: 행동 데이터의 유효성 검사
        // (스킬 사용 가능 여부, 타겟 유효성, 사용 및 대상 슬롯의 유무, 스킬의 유무, 대상의 유무 등)
        
        return returnActData;
    }
    
    public void SetTargetCharacter(CharacterHandler characterHandler)
    {
        Debug.Log("Targeted Character: " + characterHandler.name);
    }
    
}