using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    // TODO: 캐릭터 스테이터스 및 정보
    public int maxHp;
    public int maxMp;
    public int attack;
    public int defense;

    public int slotCount; // 스킬 슬롯 갯수
    
    public int characterSpeedHighLimit;
    public int characterSpeedLowLimit;
    
    public int characterCurrentSpeed;
    
    
    // 캐릭터가 보유한 스킬들의 정보
    private CharacterSkill[] characterSkills;
    
    // 장비한 장비들의 정보
    
}
