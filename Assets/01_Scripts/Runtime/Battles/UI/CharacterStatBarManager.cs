using System.Collections.Generic;
using _01_Scripts.Interfacese;
using _01_Scripts.Runtime.Battles;
using UnityEngine;

public class CharacterStatBarManager : Singleton<CharacterStatBarManager>
{
    [SerializeField] private CharacterStatBarUI barPrefab;
    [SerializeField] private Transform worldBarParent; // BattleUI 캔버스 하위
    [SerializeField] private Transform menuDockSlot;   // ComandMenu 하위, 도킹 자리

    private readonly Dictionary<CharacterHandler, CharacterStatBarUI> bars = new();

    public void SpawnBars(CharacterHandler[] playerCharacters, CharacterHandler[] enemyCharacters)
    {
        foreach (var handler in playerCharacters) Spawn(handler);
        foreach (var handler in enemyCharacters) Spawn(handler);
    }

    private void Spawn(CharacterHandler handler)
    {
        // 프리팹/씬 배치가 아직 안 되어 있으면(에디터 작업 전) 전투 자체가 막히지 않도록 건너뛴다.
        if (barPrefab == null || worldBarParent == null)
        {
            Debug.LogWarning("[CharacterStatBarManager] barPrefab/worldBarParent가 설정되지 않아 체력바를 생성하지 않습니다.");
            return;
        }

        CharacterStatBarUI bar = Instantiate(barPrefab);
        bar.Initialize(handler, worldBarParent);
        bars[handler] = bar;
    }

    public void ClearBars()
    {
        foreach (var bar in bars.Values)
            if (bar != null) Destroy(bar.gameObject);

        bars.Clear();
    }

    public void DockBarFor(CharacterHandler handler)
    {
        if (handler != null && bars.TryGetValue(handler, out var bar))
            bar.DockTo(menuDockSlot);
    }

    public void UndockBarFor(CharacterHandler handler)
    {
        if (handler != null && bars.TryGetValue(handler, out var bar))
            bar.Undock();
    }
}
