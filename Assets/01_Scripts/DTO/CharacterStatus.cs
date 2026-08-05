using System;
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

    // EquipmentSlotType 기준으로 장착 필드를 매핑한다 (InventoryManager 등 외부에서 필드명을 직접 다루지 않도록).
    // 참고: EquipmentSlotType.Accessory는 하나뿐이라 accessory1에만 매핑됨 — accessory2는 기존부터 이 API로는 못 건드림 (기존 설계 한계, 이번 작업 범위 밖).
    public void SetEquipment(EquipmentSlotType slot, IEquipment equipment)
    {
        switch (slot)
        {
            case EquipmentSlotType.RightHand:  rightHand = equipment; break;
            case EquipmentSlotType.LeftHand:   leftHand = equipment; break;
            case EquipmentSlotType.Head:       head = equipment; break;
            case EquipmentSlotType.Body:       body = equipment; break;
            case EquipmentSlotType.Legs:       legs = equipment; break;
            case EquipmentSlotType.Accessory:  accessory1 = equipment; break;
        }
    }

    public IEquipment GetEquipment(EquipmentSlotType slot)
    {
        return slot switch
        {
            EquipmentSlotType.RightHand => rightHand,
            EquipmentSlotType.LeftHand  => leftHand,
            EquipmentSlotType.Head      => head,
            EquipmentSlotType.Body      => body,
            EquipmentSlotType.Legs      => legs,
            EquipmentSlotType.Accessory => accessory1,
            _ => null
        };
    }

}
}
