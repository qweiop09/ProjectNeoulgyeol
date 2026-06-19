using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class WorldPartyManager : Singleton<WorldPartyManager>
    {
        [SerializeField] private List<CharacterData> _partyDefinitions = new();

        private List<CharacterBattleData> _runtimeParty = new();

        public IReadOnlyList<CharacterBattleData> RuntimeParty => _runtimeParty;

        protected override void Awake()
        {
            base.Awake();
            BuildRuntimeParty();
        }

        public CharacterBattleData[] GetBattleParty() => _runtimeParty.ToArray();

        // 전투 결과(현재 HP/MP/스태미나)를 월드 파티에 반영
        public void ApplyBattleResults(CharacterBattleData[] survivedParty)
        {
            for (int i = 0; i < survivedParty.Length && i < _runtimeParty.Count; i++)
            {
                _runtimeParty[i].currentHp = survivedParty[i].currentHp;
                _runtimeParty[i].currentMp = survivedParty[i].currentMp;
                _runtimeParty[i].currentStamina = survivedParty[i].currentStamina;
            }
        }

        // 패배 시 파티 상태를 최대치로 초기화
        public void ResetParty() => BuildRuntimeParty();

        public void AddMember(CharacterData character)
        {
            _partyDefinitions.Add(character);
            _runtimeParty.Add(new CharacterBattleData(character));
        }

        public void RemoveMember(CharacterData character)
        {
            int idx = _partyDefinitions.IndexOf(character);
            if (idx < 0) return;
            _partyDefinitions.RemoveAt(idx);
            _runtimeParty.RemoveAt(idx);
        }

        private void BuildRuntimeParty()
        {
            _runtimeParty.Clear();
            foreach (var def in _partyDefinitions)
                _runtimeParty.Add(new CharacterBattleData(def));
        }
    }
}
