using UnityEngine;
using UnityEngine.Serialization;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.Close;
using _01_Scripts.Runtime.Battles.Compete;
using _01_Scripts.Runtime.Battles.Decision;
using _01_Scripts.Runtime.Battles.Phase.Decision;
using _01_Scripts.Runtime.Battles.Phase.Open;

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

    [SerializeField] private int maxPartyMembers;
    
    // data Variables
    [SerializeField] private CharacterHandler originCharacter;
    
    [SerializeField] private CharacterHandler[] playerCharacters;
    [SerializeField] private CharacterHandler[] enemyCharacters;
    
    // debug Variables
    [SerializeField] private GameObject StartButton;
    [SerializeField] private CharacterData[] testFlendlyCharacterDatas;
    [SerializeField] private CharacterData[] testEnemyCharacterDatas;
    
    
    // private Methods
    public void TestStart()
    {
        StartButton.SetActive(false);
        
        CharacterBattleData[] a;
        CharacterBattleData[] b; 
        
        a = ChangeCharacterDataToCharacterBattleData(testFlendlyCharacterDatas);
        b = ChangeCharacterDataToCharacterBattleData(testEnemyCharacterDatas);
                        
        BattleStart(a, b);
    }

    // 넘겨 받는 데이터는 편성 순서대로 배열되어 있음
    public void BattleStart(CharacterBattleData[] _playerBattleDatas, CharacterBattleData[] _enemyBattleDatas)
    {
        if(_playerBattleDatas.Length > maxPartyMembers
           || _enemyBattleDatas.Length > maxPartyMembers)
        {
            Debug.LogError("파티원 수가 최대 파티원 수를 초과했습니다.");
            return;
        }
        
        Debug.Log(_playerBattleDatas);
        
        playerCharacters = new CharacterHandler[_playerBattleDatas.Length];
        enemyCharacters = new CharacterHandler[_enemyBattleDatas.Length];
        
        // 플레이어 캐릭터 핸들러 생성 및 배틀 데이터 초기화
        for(int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i] = Instantiate(originCharacter, transform);
            playerCharacters[i].SetCharacterBattleData(_playerBattleDatas[i]);
            SetRefCharacterBattleData(playerCharacters[i]);
            playerCharacters[i].characterType = CharacterHandler.CharacterType.Friendly;
        }
        
        for(int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i] = Instantiate(originCharacter, transform);
            enemyCharacters[i].SetCharacterBattleData(_enemyBattleDatas[i]);
            SetRefCharacterBattleData(enemyCharacters[i]);
            enemyCharacters[i].characterType = CharacterHandler.CharacterType.Enemy;
        }
        
        // 전투 시작 신호 보내기
        battlePhaseCoordinator.BattleStart(playerCharacters, enemyCharacters);
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
    
    // 배틀 데이터 기반의 참조 데이터 설정 (ex. 타겟의 트랜스폼)
    public CharacterHandler SetRefCharacterBattleData(CharacterHandler characterHandlers)
    {
        CharacterBattleData _characterBattleData;
        
        _characterBattleData = characterHandlers.GetCharacterBattleData();
        
        _characterBattleData.CharacterTransform = characterHandlers.transform;
        _characterBattleData.TargetingData = new ActData[_characterBattleData.CharacterData.slotCount];

        return characterHandlers;
    }
    
}
}
