using System;
using System.Collections.Generic;
using _01_Scripts.DTO.Item;
using UnityEngine;

namespace _01_Scripts.DTO
{
public enum CharacterState
{
    Normal,
    Staggered,
    Dead
}

public class CharacterStatus
{
    // 캐릭터의 지속되는(월드/전투를 넘나드는) 런타임 상태를 담는다.
    // 전투 1회용 데이터(속도, 턴 순서, 타겟팅 등)는 CharacterHandler가 별도로 들고 있는다.

    public CharacterData CharacterData;

    public int currentHp;
    public int currentMp;
    public int currentStamina;

    public CharacterState currentState;

    public void SetCurrentState(CharacterState newState)
    {
        Debug.Log("상태 변경: " + currentState + " -> " + newState);

        currentState = newState;
    }

    // 캐릭터가 장착한 장비 (인스턴스별로 소유 - CharacterData(SO)는 공유 템플릿이라 여기 두면 안 됨)
    public IEquipment rightHand;
    public IEquipment leftHand;
    public IEquipment head;
    public IEquipment body;
    public IEquipment legs;
    public IEquipment accessory1;
    public IEquipment accessory2;

    // 캐릭터가 소지한 아이템 목록
    public List<Item.Item> inventory = new List<Item.Item>(15);


    public CharacterStatus(CharacterData characterData)
    {
        CharacterData = characterData;

        currentHp = CharacterData.maxHp;
        currentMp = CharacterData.maxMp;
        currentStamina = CharacterData.maxStamina;
    }

    // 장비 보너스를 반영한 실질 최대치/전투 스탯
    public int GetMaxHp() => CharacterData.maxHp + SumEquipment(e => e.MaxHpBonus);
    public int GetMaxMp() => CharacterData.maxMp + SumEquipment(e => e.MaxMpBonus);
    public int GetMaxStamina() => CharacterData.maxStamina + SumEquipment(e => e.MaxStaminaBonus);
    public int GetAttack() => CharacterData.attack + SumEquipment(e => e.AttackBonus);
    public int GetDefense() => CharacterData.defense + SumEquipment(e => e.DefenseBonus);

    private int SumEquipment(Func<IEquipment, int> selector)
    {
        int sum = 0;
        foreach (var equipment in new[] { rightHand, leftHand, head, body, legs, accessory1, accessory2 })
        {
            if (equipment != null)
                sum += selector(equipment);
        }

        return sum;
    }

}
}
