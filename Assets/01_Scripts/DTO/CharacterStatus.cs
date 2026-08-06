using System;
using System.Collections.Generic;
using _01_Scripts.DTO.Item;
using _01_Scripts.DTO.Item.Effects;
using _01_Scripts.Runtime.Worlds;
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

    // 현재 걸려있는 지속 효과(버프/디버프). 라운드 종료마다 ClosePhaseController가 감소·소멸시키고,
    // 전투 종료 시 BattleManager가 비운다(안 그러면 다음 전투/월드맵까지 새어나감).
    public List<ActiveBuff> activeBuffs = new();

    // 이 캐릭터를 만든 EnemyData 역참조 (적 전용, 플레이어는 항상 null) — 컷신 오버라이드 등 인카운터
    // 시점에만 아는 정보를 전투 중에도 조회할 수 있게 해준다. BattleManager.BattleStart에서 채워짐.
    public EnemyData SourceEnemyData;

    public CharacterStatus(CharacterData characterData)
    {
        CharacterData = characterData;

        currentHp = CharacterData.maxHp;
        currentMp = CharacterData.maxMp;
        currentStamina = CharacterData.maxStamina;
    }

    // 장비 보너스 + 활성 버프를 반영한 실질 최대치/전투 스탯
    public int GetMaxHp() => CharacterData.maxHp + SumEquipment(e => e.MaxHpBonus) + SumBuffs(BuffStatType.MaxHp);
    public int GetMaxMp() => CharacterData.maxMp + SumEquipment(e => e.MaxMpBonus) + SumBuffs(BuffStatType.MaxMp);
    public int GetMaxStamina() => CharacterData.maxStamina + SumEquipment(e => e.MaxStaminaBonus) + SumBuffs(BuffStatType.MaxStamina);
    public int GetAttack() => CharacterData.attack + SumEquipment(e => e.AttackBonus) + SumBuffs(BuffStatType.Attack);
    public int GetDefense() => CharacterData.defense + SumEquipment(e => e.DefenseBonus) + SumBuffs(BuffStatType.Defense);

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

    private int SumBuffs(BuffStatType statType)
    {
        int sum = 0;
        foreach (ActiveBuff buff in activeBuffs)
        {
            if (buff.Source is StatBuffEffect statBuff && statBuff.statType == statType)
                sum += buff.ResolvedAmount;
        }

        return sum;
    }

    // 최대치가 바뀔 때(버프 부여/만료) 현재치를 처리한다.
    // scaleProportionally && 늘어난 경우: 이전 비율을 유지하며 현재치도 같이 늘림.
    // 그 외(줄어들 때는 항상): 새 최대치를 넘지 않도록 클램프만 한다.
    public void OnMaxHpChanged(int previousMax, bool scaleProportionally)
    {
        int newMax = GetMaxHp();
        if (newMax == previousMax) return;

        if (scaleProportionally && newMax > previousMax && previousMax > 0)
            currentHp = Mathf.RoundToInt(currentHp * (newMax / (float)previousMax));

        currentHp = Mathf.Clamp(currentHp, 0, newMax);
    }

    public void OnMaxMpChanged(int previousMax, bool scaleProportionally)
    {
        int newMax = GetMaxMp();
        if (newMax == previousMax) return;

        if (scaleProportionally && newMax > previousMax && previousMax > 0)
            currentMp = Mathf.RoundToInt(currentMp * (newMax / (float)previousMax));

        currentMp = Mathf.Clamp(currentMp, 0, newMax);
    }

    public void OnMaxStaminaChanged(int previousMax, bool scaleProportionally)
    {
        int newMax = GetMaxStamina();
        if (newMax == previousMax) return;

        if (scaleProportionally && newMax > previousMax && previousMax > 0)
            currentStamina = Mathf.RoundToInt(currentStamina * (newMax / (float)previousMax));

        currentStamina = Mathf.Clamp(currentStamina, 0, newMax);
    }

    // EquipmentSlotType 기준으로 장착 필드를 매핑한다 (InventoryManager 등 외부에서 필드명을 직접 다루지 않도록).
    public void SetEquipment(EquipmentSlotType slot, IEquipment equipment)
    {
        switch (slot)
        {
            case EquipmentSlotType.RightHand:      rightHand = equipment; break;
            case EquipmentSlotType.LeftHand:       leftHand = equipment; break;
            case EquipmentSlotType.Head:           head = equipment; break;
            case EquipmentSlotType.Body:           body = equipment; break;
            case EquipmentSlotType.Legs:           legs = equipment; break;
            case EquipmentSlotType.AccessoryRight: accessory1 = equipment; break;
            case EquipmentSlotType.AccessoryLeft:  accessory2 = equipment; break;
        }
    }

    public IEquipment GetEquipment(EquipmentSlotType slot)
    {
        return slot switch
        {
            EquipmentSlotType.RightHand      => rightHand,
            EquipmentSlotType.LeftHand       => leftHand,
            EquipmentSlotType.Head           => head,
            EquipmentSlotType.Body           => body,
            EquipmentSlotType.Legs           => legs,
            EquipmentSlotType.AccessoryRight => accessory1,
            EquipmentSlotType.AccessoryLeft  => accessory2,
            _ => null
        };
    }

}
}
