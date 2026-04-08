using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    // class Variables
    [SerializeField] private OpenPhaseController openPhaseController;
    [SerializeField] private DecisionPhaseController decisionPhaseController;
    [SerializeField] private CompetePhaseController competePhaseController;
    [SerializeField] private ClosePhaseController closePhaseController;
    
    [SerializeField] private BattlePhaseCoordinator battlePhaseCoordinator;
    
    // data Variables
    [SerializeField] private CharacterBattleData[] playerCharacters;
    [SerializeField] private CharacterBattleData[] enemyCharacters;
    
    // debug Variables
    [SerializeField] private CharacterStatus testCharacterStatus;
    
    // private Methods
    public void TestStart()
    {
        playerCharacters = new CharacterBattleData[] { ChangeCharacterDataToCharacterBattleData(testCharacterStatus)} ;
        enemyCharacters = new CharacterBattleData[] {ChangeCharacterDataToCharacterBattleData(testCharacterStatus)} ;
        
        BattleStart(playerCharacters
            , enemyCharacters);
    }
    
    // 넘겨 받는 데이터는 편성 순서대로 배열되어 있음
    public void BattleStart(CharacterBattleData[] _playerBattleDatas, CharacterBattleData[] _enemyBattleDatas)
    {
        Debug.Log(_playerBattleDatas);
        
        battlePhaseCoordinator.BattleStart(_playerBattleDatas, _enemyBattleDatas);
    }
    
    // public Methods
     public BattlePhaseCoordinator GetBattlePhaseCoordinator()
    {
        return battlePhaseCoordinator;
    }

    public CharacterBattleData[] GetPlayerCharacters()
    {
        return playerCharacters;
    }

    public CharacterBattleData[] GetEnemyCharacters()
    {
        return enemyCharacters;
    }
    
    // private Methods
    private CharacterBattleData[] ChangeCharacterDataToCharacterBattleData(CharacterStatus[] _characterStatuses)
    {
        CharacterBattleData[] _returnBattleDataArray = new CharacterBattleData[_characterStatuses.Length];
        
        for (int i = 0; i < _characterStatuses.Length; i++)
        {
            _returnBattleDataArray[i] = new CharacterBattleData(_characterStatuses[i]);
        }

        return _returnBattleDataArray;
    }
    
    private CharacterBattleData ChangeCharacterDataToCharacterBattleData(CharacterStatus _characterStatuses)
    {
        
        return  new CharacterBattleData(_characterStatuses);
                         
    }
    
}
