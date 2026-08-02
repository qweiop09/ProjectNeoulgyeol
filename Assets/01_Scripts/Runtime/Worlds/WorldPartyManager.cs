using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class WorldPartyManager : Singleton<WorldPartyManager>
    {
        [SerializeField] private List<CharacterData> _partyDefinitions = new();

        private List<CharacterStatus> _runtimeParty = new();

        public IReadOnlyList<CharacterStatus> RuntimeParty => _runtimeParty;

        protected override void Awake()
        {
            base.Awake();
            BuildRuntimeParty();
        }

        // 같은 CharacterStatus 참조를 그대로 반환한다 (복사 아님).
        // 전투 중 CharacterStatusCalculator가 만든 변경이 곧바로 _runtimeParty에 반영되므로
        // 별도의 결과 반영(field copy-back) 단계가 필요 없다.
        public CharacterStatus[] GetBattleParty() => _runtimeParty.ToArray();

        // 패배 시 파티 상태를 최대치로 초기화
        public void ResetParty() => BuildRuntimeParty();

        public void AddMember(CharacterData character)
        {
            _partyDefinitions.Add(character);
            _runtimeParty.Add(new CharacterStatus(character));
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
                _runtimeParty.Add(new CharacterStatus(def));
        }
    }
}
