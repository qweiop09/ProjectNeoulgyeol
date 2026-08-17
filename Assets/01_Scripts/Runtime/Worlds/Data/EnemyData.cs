using System;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting;
using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Worlds
{
    [Serializable]
    public class EnemyDropEntry
    {
        public Item item;
        [Range(0f, 1f)] public float dropRate = 0.5f;
        [Min(1)] public int minQuantity = 1;
        [Min(1)] public int maxQuantity = 1;
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "World/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public CharacterData characterData;

        [Header("AI")]
        [Tooltip("비워두면 EnemyActionController의 defaultProfile로 대체됨(테스트 배틀 등 SourceEnemyData가 없는 경우 대비).")]
        public EnemyAIProfile aiProfile;

        [Header("Drop Table")]
        public EnemyDropEntry[] dropTable;

        [Header("컷신 (미구현 — 훅만 마련)")]
        [Tooltip("이 적이 전투에 하나라도 포함되면 컷신 트리거 대상. 컷신 재생 로직 자체는 아직 없음 — 지금은 감지만 하고 로그만 남김.")]
        public TimelineAsset entryCutscene;
    }
}
