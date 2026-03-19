using System.Collections.Generic;
using UnityEngine;

public class OpenPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;

    
    CharacterBattleData[] playerCharacters;
    CharacterBattleData[] enemyCharacters;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnBattleStart += (data1, data2)
            => StartBattle(data1, data2);
        battlePhaseCoordinator.OnOpenPhaseStart += StartOpenPhase;
    }

    private void StartBattle(CharacterBattleData[] _playerCharacters, CharacterBattleData[] _enemyCharacters)
    {
        playerCharacters = _playerCharacters;
        enemyCharacters =  _enemyCharacters;
        
        StartOpenPhase();
    }
    
    // 개막 페이즈 로직 구현
    private void StartOpenPhase()
    {
        
        playerCharacters = SetCharactersSpeed(playerCharacters);
        enemyCharacters = SetCharactersSpeed(enemyCharacters);

        SortByCurrentSpeedCharacterBattleDatas(playerCharacters);
        SortByCurrentSpeedCharacterBattleDatas(enemyCharacters);
        
        Debug.Log("속도 조정 완료 \n -------------------------------------------------------");
        
        Debug.Log(" 아군 : ");
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].DebugPrintStatusData();
        } 
        
        Debug.Log("적군 : ");
        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i].DebugPrintStatusData();
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
    

    private CharacterBattleData[] SetCharactersSpeed(CharacterBattleData[] _characterBattleDatas)
    {
        CharacterBattleData[] _returnBattleDataArray = new CharacterBattleData[_characterBattleDatas.Length];

        for (int i = 0; i < _characterBattleDatas.Length; i++)
        {
            _characterBattleDatas[i].SetRandomSpeed();
            
            _returnBattleDataArray[i] = _characterBattleDatas[i];
        }
        
        return _returnBattleDataArray;
    }

    private CharacterBattleData[] SortByCurrentSpeedCharacterBattleDatas(CharacterBattleData[] _characterBattleDatas)
    {
        if (_characterBattleDatas == null || _characterBattleDatas.Length <= 1)
            return _characterBattleDatas;

        void QuickSort(int left, int right)
        {
            int i = left;
            int j = right;
            int pivot = _characterBattleDatas[(left + right) / 2].GetCurrentSpeed;

            while (i <= j)
            {
                while (_characterBattleDatas[i].GetCurrentSpeed > pivot) i++;
                while (_characterBattleDatas[j].GetCurrentSpeed < pivot) j--;

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

    
    private CharacterBattleData[] ChangeCharacterDataToCharacterBattleData(CharacterStatus[] _characterStatuses)
    {
        CharacterBattleData[] _returnBattleDataArray = new CharacterBattleData[_characterStatuses.Length];
        
        for (int i = 0; i < _characterStatuses.Length; i++)
        {
            _returnBattleDataArray[i] = new CharacterBattleData(_characterStatuses[i]);
        }

        return _returnBattleDataArray;
    }
    
    
}
