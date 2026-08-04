using UnityEngine;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles.Close;
using _01_Scripts.Runtime.Battles.Compete;
using _01_Scripts.Runtime.Battles.Decision;
using _01_Scripts.Runtime.Battles.Phase.Decision;
using _01_Scripts.Runtime.Battles.Phase.Open;
using _01_Scripts.Runtime.Worlds;
using _01_Scripts.Runtime.Worlds.Loot;
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

    private EnemyData[] _enemyDatas;

    private void Start()
    {
        if (BattleContext.PlayerParty == null || BattleContext.EnemyParty == null) return;

        var playerParty = BattleContext.PlayerParty;
        var enemyParty = BattleContext.EnemyParty;
        _enemyDatas = BattleContext.EnemyDatas;   // 드랍 계산을 위해 씬 로드 전 저장
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

        // CharacterStatus는 참조로 공유되므로 여기서 만드는 배열은 상태 동기화용이 아니라
        // 전리품 계산/결과 리포팅(BattleResult.SurvivedParty)을 위한 목적으로만 쓰인다.
        var survivedParty = new CharacterStatus[playerCharacterHandlers.Length];
        for (int i = 0; i < playerCharacterHandlers.Length; i++)
            survivedParty[i] = playerCharacterHandlers[i].GetCharacterStatus();

        LootResult loot = isWin ? LootCalculator.Calculate(_enemyDatas) : null;
        BattleContext.SetResult(new BattleResult { IsWin = isWin, SurvivedParty = survivedParty, Loot = loot });

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

        CharacterStatus[] a;
        CharacterStatus[] b;

        a = ChangeCharacterDataToCharacterStatus(testFlendlyCharacterDatas);
        b = ChangeCharacterDataToCharacterStatus(testEnemyCharacterDatas);
        
        for(int i = 0; i < a.Length; i++)
            for(int ii = 0; ii < 6; ii++)
                a[i].inventory.Add(testItem1);
                        
        BattleStart(a, b);
    }

    // 넘겨 받는 데이터는 편성 순서대로 배열되어 있음
    public void BattleStart(CharacterStatus[] _playerBattleDatas, CharacterStatus[] _enemyBattleDatas)
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
            playerCharacters[i].SetCharacterStatus(_playerBattleDatas[i]);
            SetRefCharacterBattleData(playerCharacters[i]);
            playerCharacters[i].characterType = CharacterHandler.CharacterType.Friendly;
        }

        for(int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i] = Instantiate(originCharacter, transform);
            enemyCharacters[i].SetCharacterStatus(_enemyBattleDatas[i]);
            SetRefCharacterBattleData(enemyCharacters[i]);
            enemyCharacters[i].characterType = CharacterHandler.CharacterType.Enemy;
        }

        CharacterStatBarManager.Instance.SpawnBars(playerCharacters, enemyCharacters);

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
        CharacterStatBarManager.Instance.ClearBars();

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
    private CharacterStatus[] ChangeCharacterDataToCharacterStatus(CharacterData[] _characterStatuses)
    {
        CharacterStatus[] _returnBattleDataArray = new CharacterStatus[_characterStatuses.Length];

        for (int i = 0; i < _characterStatuses.Length; i++)
        {
            _returnBattleDataArray[i] = new CharacterStatus(_characterStatuses[i]);
        }

        return _returnBattleDataArray;
    }

    // 전투 1회용 데이터 초기화 (ex. 슬롯 수에 맞춘 타겟팅 배열)
    private CharacterHandler SetRefCharacterBattleData(CharacterHandler characterHandlers)
    {
        characterHandlers.TargetingData = new ActData[characterHandlers.GetCharacterStatus().CharacterData.slotCount];

        return characterHandlers;
    }
    
}
}
