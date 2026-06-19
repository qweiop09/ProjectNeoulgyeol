using UnityEngine;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles.Close;
using _01_Scripts.Runtime.Battles.Compete;
using _01_Scripts.Runtime.Battles.Decision;
using _01_Scripts.Runtime.Battles.Phase.Decision;
using _01_Scripts.Runtime.Battles.Phase.Open;
using _01_Scripts.Runtime.Worlds;
using TMPro;

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
    
    [SerializeField] private Item testItem1;
    
    [SerializeField] private TextMeshProUGUI endText;

    private void Start()
    {
        if (BattleContext.PlayerParty == null || BattleContext.EnemyParty == null) return;

        var playerParty = BattleContext.PlayerParty;
        var enemyParty = BattleContext.EnemyParty;
        BattleContext.ClearEncounter();

        StartButton.SetActive(false);
        BattleStart(playerParty, enemyParty);
    }

    public void OnEnable()
    {
        battlePhaseCoordinator.OnBattleEnd += BattleEnd;
    }
    
    private void OnDisable()
    {
        battlePhaseCoordinator.OnBattleEnd -= BattleEnd;
    }
    
    private void BattleEnd(CharacterHandler[] playerCharacterHandlers, bool isWin)
    {
        endText.text = isWin ? "You Win!" : "You Lose!";
        ClearCharacters();

        var survivedParty = new CharacterBattleData[playerCharacterHandlers.Length];
        for (int i = 0; i < playerCharacterHandlers.Length; i++)
            survivedParty[i] = playerCharacterHandlers[i].GetCharacterBattleData();

        BattleContext.SetResult(new BattleResult { IsWin = isWin, SurvivedParty = survivedParty });

        string worldScene = BattleContext.WorldSceneName;
        if (string.IsNullOrEmpty(worldScene))
        {
            Debug.LogWarning("[BattleManager] 월드 씬 이름이 없습니다. 직접 실행 시에는 BattleContext가 비어있습니다.");
            return;
        }

        SceneLoader.Instance.LoadScene(worldScene);
    }


    // private Methods
    public void TestStart()
    {
        StartButton.SetActive(false);
        
        CharacterBattleData[] a;
        CharacterBattleData[] b; 
        
        a = ChangeCharacterDataToCharacterBattleData(testFlendlyCharacterDatas);
        b = ChangeCharacterDataToCharacterBattleData(testEnemyCharacterDatas);
        
        for(int i = 0; i < a.Length; i++)
            for(int ii = 0; ii < 6; ii++)
                a[i].inventory.Add(testItem1);
                        
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

        ClearCharacters();

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

    private void ClearCharacters()
    {
        if (playerCharacters != null)
        {
            foreach (var c in playerCharacters)
                if (c != null) Destroy(c.gameObject);
            playerCharacters = null;
        }

        if (enemyCharacters != null)
        {
            foreach (var c in enemyCharacters)
                if (c != null) Destroy(c.gameObject);
            enemyCharacters = null;
        }
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
    private CharacterHandler SetRefCharacterBattleData(CharacterHandler characterHandlers)
    {
        CharacterBattleData _characterBattleData;
        
        _characterBattleData = characterHandlers.GetCharacterBattleData();
        
        _characterBattleData.CharacterTransform = characterHandlers.transform;
        _characterBattleData.TargetingData = new ActData[_characterBattleData.CharacterData.slotCount];

        return characterHandlers;
    }
    
}
}
