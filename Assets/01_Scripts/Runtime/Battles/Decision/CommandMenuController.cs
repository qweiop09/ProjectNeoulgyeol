using System;
using _01_Scripts.Runtime.Battles;
using _01_Scripts.Runtime.Battles.Decision;
using Unity.VisualScripting;
using UnityEngine;

public class CommandMenuController : MonoBehaviour
{
    // 행동대상을 선택해야 할 때 외부에 알리는 이벤트
    public event Action CompletedActionSetting; 
    
    public void PressedAttackButton()
    {
        Debug.Log("Action Button Pressed");
        CompletedActionSetting?.Invoke();
    }
    
    // 아래에 UI클릭 메소드 추가
    
    public void SetTargetCharacter(CharacterHandler characterHandler)
    {
        Debug.Log("Targeted Character: " + characterHandler.name);
    }
    
    // ui 클릭 시 메서드 할당
    
}