using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattleData
{
    // TODO: 캐릭터 스테이터스 및 정보
    
    public CharacterStatus characterStatus;
    
    private int currentHp;
    private int currentMp;
    
    private int currentSpeed;
    public int GetCurrentSpeed => currentSpeed;
    public void SetCurrentSpeed(int _speed) { currentSpeed = _speed; }

    private int placementOrder;
    public int GetPlacementOrder => placementOrder;
    public void SetPlacementOrder(int _order) { placementOrder = _order; }
    
    
    // 캐릭터가 보유한 스킬들의 정보
    private CharacterSkill[] characterSkills;
    
    // 사용하는 행동의 정보 ( 사용 스킬, 대상 등 )
    private TargetingData[] targetingData;
    
    
    private Transform characterTransform;
    public Transform GetCharacterTransform => characterTransform;
    public void SetCharacterTransform(Transform _transform) { characterTransform = _transform; }
    

    public CharacterBattleData(CharacterStatus _characterStatus)
    {
        characterStatus = _characterStatus;
        
        currentHp = characterStatus.maxHp;
        currentMp = characterStatus.maxMp;
    }
    
    
    public class TargetingData
    {
        public int useSlot;
        public int targetPosition;
        public int targetSlot;

        public CharacterSkill useSkill;

        public TargetingData(int _useSlot, int _targetPosition, int _targetSlot)
        {
            useSlot = _useSlot;
            targetPosition = _targetPosition;
            targetSlot = _targetSlot;
        }
    }
    
    public void SetRandomSpeed()
    {
        currentSpeed = characterStatus.GetRandomSpeed();
    }

    public void SetTargetingSlot(int _slotIndex, TargetingData _targetingData)
    {
        if (targetingData.Length -1 < _slotIndex)
        {
            Debug.LogError("Error : 수정하려는 슬롯의 위치가 존재하지 않습니다.");
            return;
        }
        
        targetingData[_slotIndex] = _targetingData;

    }

    public void SetTargetingSlot(int _slotIndex, int _useSlot, int _targetPosition, int _targetSlot)
    {
        if (targetingData.Length -1 < _slotIndex)
        {
            Debug.LogError("Error : 수정하려는 슬롯의 위치가 존재하지 않습니다.");
            return;
        }
        
        targetingData[_slotIndex] = new TargetingData(_useSlot, _targetPosition, _targetSlot);
        
    }
    
    public void SetTargetingSlot(int _slotIndex, int _useSlot, CharacterSkill _useSkill, int _targetPosition, int _targetSlot)
    {
        if (targetingData.Length -1 < _slotIndex)
        {
            Debug.LogError("Error : 수정하려는 슬롯의 위치가 존재하지 않습니다.");
            return;
        }
        
        targetingData[_slotIndex] = new TargetingData(_useSlot, _targetPosition, _targetSlot);
        targetingData[_slotIndex].useSkill = _useSkill;
    }

    public void DebugPrintStatusData()
    {
        Debug.Log(
        "체력 : " + characterStatus.maxHp + "\n" +
                "마나 : " + characterStatus.maxMp + "\n" +
                "공격 : " + characterStatus.attack + "\n" +
                "방어 : " + characterStatus.defense + "\n" +
                "현재 속도 : " + currentSpeed );
    }
    

}
