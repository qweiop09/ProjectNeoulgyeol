using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class OpenPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    [SerializeField] private CharacterHandler characterHandlerPrefab;
    
    private CharacterHandler[] playerCharacters;
    private CharacterHandler[] enemyCharacters;
    
    [SerializeField] private Transform[] playerCharacterPositions;
    [SerializeField] private Transform[] enemyCharacterPositions;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnBattleStart += (data1, data2)
            => StartBattle(data1, data2);
        battlePhaseCoordinator.OnOpenPhaseStart += StartOpenPhase;
    }

    private void StartBattle(CharacterBattleData[] _playerCharacters, CharacterBattleData[] _enemyCharacters)
    {
        playerCharacters = new CharacterHandler[_playerCharacters.Length];
        enemyCharacters = new CharacterHandler[_enemyCharacters.Length];

        for (int i = 0; i < _playerCharacters.Length; i++)
        {
            playerCharacters[i] = Instantiate(characterHandlerPrefab);
            playerCharacters[i].characterType = CharacterHandler.CharacterType.Friendly;
        }

        for (int i = 0; i < _enemyCharacters.Length; i++)
        {
            enemyCharacters[i] = Instantiate(characterHandlerPrefab);
            enemyCharacters[i].characterType = CharacterHandler.CharacterType.Enemy;
        }

        playerCharacters = SetBattleDataToHandlers(playerCharacters , _playerCharacters);
        enemyCharacters = SetBattleDataToHandlers(enemyCharacters , _enemyCharacters);
        
        StartOpenPhase();
    }
    
    // 개막 페이즈 로직 구현
    private void StartOpenPhase()
    {
        Debug.Log("Start Open Phase");
        
        playerCharacters = SetCharactersSpeed(playerCharacters);
        enemyCharacters = SetCharactersSpeed(enemyCharacters);

        SortByCurrentSpeedCharacterBattleDatas(playerCharacters);
        SortByCurrentSpeedCharacterBattleDatas(enemyCharacters);
        
        SetCharactersPosition(playerCharacters, playerCharacterPositions);
        SetCharactersPosition(enemyCharacters, enemyCharacterPositions);
        
        Debug.Log("속도 조정 완료 \n -------------------------------------------------------");
        
        Debug.Log(" 아군 : ");
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].GetCharacterBattleData().DebugPrintStatusData();
        } 
        
        Debug.Log("적군 : ");
        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i].GetCharacterBattleData().DebugPrintStatusData();
        }
        
        CompleteOpenPhaseProcess();
    }
    
    // 개막 페이즈 로직 동작 완료
    private void CompleteOpenPhaseProcess()
    {
        // Open Phase Logic
        Debug.Log("Open Phase Started");
        
        // Open Phase End
        battlePhaseCoordinator.CompleteOpenPhaseStart();
    }
    
    private void SetCharactersPosition(CharacterHandler[] _characterBattleDatas, Transform[] _characterPositions)
    {
        for (int i = 0; i < _characterBattleDatas.Length; i++)
        {
            // 캐릭터 위치 설정
             _characterBattleDatas[i].transform.position = _characterPositions[i].position;
             if (_characterBattleDatas[i].characterType == CharacterHandler.CharacterType.Friendly)
                 _characterBattleDatas[i].transform.rotation = new Quaternion(0, 0, 0, 0);
             else
                 _characterBattleDatas[i].transform.rotation = new Quaternion(0, 180, 0, 0);
        }
    }

    private CharacterHandler[] SetCharactersSpeed(CharacterHandler[] _characterBattleDatas)
    {
        CharacterHandler[] _returnBattleDataArray = new CharacterHandler[_characterBattleDatas.Length];

        for (int i = 0; i < _characterBattleDatas.Length; i++)
        {
            _characterBattleDatas[i].GetCharacterBattleData().SetRandomSpeed();
            
            _returnBattleDataArray[i] = _characterBattleDatas[i];
        }
        
        return _returnBattleDataArray;
    }

    private CharacterHandler[] SortByCurrentSpeedCharacterBattleDatas(CharacterHandler[] _characterBattleDatas)
    {
        if (_characterBattleDatas == null || _characterBattleDatas.Length <= 1)
            return _characterBattleDatas;

        void QuickSort(int left, int right)
        {
            int i = left;
            int j = right;
            int pivot = _characterBattleDatas[(left + right) / 2].GetCharacterBattleData().CurrentSpeed;

            while (i <= j)
            {
                while (_characterBattleDatas[i].GetCharacterBattleData().CurrentSpeed > pivot) i++;
                while (_characterBattleDatas[j].GetCharacterBattleData().CurrentSpeed < pivot) j--;

                if (i <= j)
                {
                    (_characterBattleDatas[i], _characterBattleDatas[j]) 
                        = (_characterBattleDatas[j], _characterBattleDatas[i]);
                    i++;
                    j--;
                }
            }

            if (left < j) QuickSort(left, j);
            if (i < right) QuickSort(i, right);
        }

        QuickSort(0, _characterBattleDatas.Length - 1);

        return _characterBattleDatas;
    }

    private CharacterHandler[] SetBattleDataToHandlers(CharacterHandler[] _characterHandlers , CharacterBattleData[] _characterBattleDatas)
    {
        for( int i = 0; i < _characterHandlers.Length; i++)
            _characterHandlers[i].SetCharacterBattleData(_characterBattleDatas[i]);
        
        return _characterHandlers;
    }
    
    private CharacterBattleData[] ChangeCharacterDataToCharacterBattleData(CharacterData[] _characterStatuses)
    {
        CharacterBattleData[] _returnBattleDataArray = new CharacterBattleData[_characterStatuses.Length];
        
        for (int i = 0; i < _characterStatuses.Length; i++)
        {
            _returnBattleDataArray[i] = new CharacterBattleData(_characterStatuses[i]);
        }

        return _returnBattleDataArray;
    }
    
    
}
