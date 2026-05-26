using UnityEngine;
using UnityEngine.Serialization;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.Close;
using _01_Scripts.Runtime.Battles.Compete;
using _01_Scripts.Runtime.Battles.Decision;

namespace _01_Scripts.Runtime.Battles
{
public class BattleManager : MonoBehaviour
{
    // class Variables
    [SerializeField] private OpenPhaseController openPhaseController;
    [SerializeField] private DecisionPhaseController decisionPhaseController;
    [SerializeField] private CompetePhaseController competePhaseController;
    [SerializeField] private ClosePhaseController closePhaseController;
    
    [SerializeField] private BattlePhaseCoordinator battlePhaseCoordinator;
    
    // data Variables
    [SerializeField] private CharacterHandler[] playerCharacters;
    [SerializeField] private CharacterHandler[] enemyCharacters;
    
    // debug Variables
    [FormerlySerializedAs("testCharacterStatus")] [SerializeField] private CharacterData testCharacterData;
    
    // private Methods
    public void TestStart()
    {
        CharacterBattleData[] a;
        CharacterBattleData[] b; 
        
        a = new CharacterBattleData[] { ChangeCharacterDataToCharacterBattleData(testCharacterData)} ;
        b = new CharacterBattleData[] { ChangeCharacterDataToCharacterBattleData(testCharacterData)} ;
                        
        BattleStart(a, b);
    }

    // 넘겨 받는 데이터는 편성 순서대로 배열되어 있음
    public void BattleStart(CharacterBattleData[] _playerBattleDatas, CharacterBattleData[] _enemyBattleDatas)
    {
        Debug.Log(_playerBattleDatas);
        
        if(playerCharacters.Length < _playerBattleDatas.Length 
           || enemyCharacters.Length < _enemyBattleDatas.Length)
        {
            Debug.LogError("Not enough character handlers for the provided battle data.");
            return;
        }

        for (int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].SetCharacterBattleData(_playerBattleDatas[i]);
            enemyCharacters[i].SetCharacterBattleData(_enemyBattleDatas[i]);
        }

        SetRefCharacterBattleData(playerCharacters);
        SetRefCharacterBattleData(enemyCharacters);
        
        battlePhaseCoordinator.BattleStart(_playerBattleDatas, _enemyBattleDatas);
    }
    
    // public Methods
    public BattlePhaseCoordinator GetBattlePhaseCoordinator()
    {
        return battlePhaseCoordinator;
    }

    public CharacterHandler[] GetPlayerCharacters()
    {
        return playerCharacters;
    }

    public CharacterHandler[] GetEnemyCharacters()
    {
        return enemyCharacters;
    }
    
    // private Methods
    private CharacterBattleData[] ChangeCharacterDataToCharacterBattleData(CharacterData[] _characterStatuses)
    {
        CharacterBattleData[] _returnBattleDataArray = new CharacterBattleData[_characterStatuses.Length];
        
        for (int i = 0; i < _characterStatuses.Length; i++)
        {
            _returnBattleDataArray[i] = new CharacterBattleData(_characterStatuses[i]);
        }

        return _returnBattleDataArray;
    }
    
    private CharacterBattleData ChangeCharacterDataToCharacterBattleData(CharacterData _characterStatuses)
    {
        
        return  new CharacterBattleData(_characterStatuses);
                         
    }
    
    // 배틀 데이터 기반의 참조 데이터 설정 (ex. 타겟의 트랜스폼)
    public CharacterHandler[] SetRefCharacterBattleData(CharacterHandler[] characterHandlers)
    {
        CharacterBattleData _characterBattleData;
        
        for (int i = 0; i < characterHandlers.Length; i++)
        {
            _characterBattleData = characterHandlers[i].GetCharacterBattleData();
            
            _characterBattleData.CharacterTransform = characterHandlers[i].transform;
            _characterBattleData.TargetingData = new ActData[_characterBattleData.CharacterData.slotCount];
        }

        return characterHandlers;
    }
    
    // 기본 데이터 기반의 참조 데이터 설정 
    private CharacterBattleData SetRefCharacterData(CharacterBattleData characterBattleData)
    {
        // pass
        return characterBattleData;
    }
    
}
}
