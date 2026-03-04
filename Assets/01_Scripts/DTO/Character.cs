using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // TODO: 캐릭터 스테이터스 및 정보
    
    private CharacterSkill[] characterSkills;
    
    
    
    
    
    private targetSlot[] targetSlots;
    
    private class targetSlot
    {
        private int targetNumber;
        public CharacterSkill useSkill;
    }

}
