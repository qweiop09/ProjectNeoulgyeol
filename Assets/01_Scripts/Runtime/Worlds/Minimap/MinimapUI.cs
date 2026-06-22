using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _01_Scripts.Runtime.Worlds.Minimap
{
    public class MinimapUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private MinimapLayoutData _layout;

        [Header("References")]
        [SerializeField] private RectTransform _panel;          // 정사각형 캔버스/패널 루트
        [SerializeField] private RectTransform _cellContainer;  // 셀이 생성될 빈 컨테이너
        [SerializeField] private MinimapCellUI _cellPrefab;
        [SerializeField] private Image _linePrefab;             // 1x1 흰색 Image 프리팹

        [Header("Style")]
        [SerializeField] private float _padding = 12f;
        [Range(0.05f, 0.4f)]
        [SerializeField] private float _gapRatio = 0.15f;      // 셀 크기 대비 간격 비율

        [Header("Toggle")]
        [SerializeField] private Key _toggleKey = Key.M;

        private readonly Dictionary<RoomData, List<MinimapCellUI>> _roomCells = new();

        private float _step;        // 격자 한 칸당 픽셀 거리 (셀 + 간격)
        private float _cellSize;    // 셀 하나의 픽셀 크기
        private Vector2Int _gridMin;
        private Vector2 _centerOffset;

        private CanvasGroup _canvasGroup;

        private void Start()
        {
            _canvasGroup = _panel != null ? _panel.GetComponent<CanvasGroup>() : null;
            BuildMinimap();

            if (MinimapManager.Instance != null)
                MinimapManager.Instance.OnUpdated += Refresh;
        }

        private void OnDestroy()
        {
            if (MinimapManager.Instance != null)
                MinimapManager.Instance.OnUpdated -= Refresh;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[_toggleKey].wasPressedThisFrame)
                ToggleVisibility();
        }

        // ──────────────────────────────────────────────
        // Build
        // ──────────────────────────────────────────────

        private void BuildMinimap()
        {
            if (_layout == null)
            {
                Debug.LogWarning("[MinimapUI] MinimapLayoutData가 지정되지 않았습니다.");
                return;
            }

            var allRooms = new List<RoomData>();
            foreach (var entry in _layout.GetAllEntries())
                if (entry.room != null) allRooms.Add(entry.room);

            MinimapManager.Instance?.RegisterRooms(allRooms);

            CalculateLayout();
            CreateAllConnectionLines();

            foreach (var entry in _layout.GetAllEntries())
                if (entry.room != null)
                    CreateRoomCells(entry);

            Refresh();
        }

        // 패널 크기와 격자 범위로부터 step, cellSize, centerOffset 계산
        private void CalculateLayout()
        {
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var entry in _layout.GetAllEntries())
            {
                foreach (var offset in GetCells(entry))
                {
                    var g = entry.anchor + offset;
                    if (g.x < minX) minX = g.x;
                    if (g.x > maxX) maxX = g.x;
                    if (g.y < minY) minY = g.y;
                    if (g.y > maxY) maxY = g.y;
                }
            }

            _gridMin = new Vector2Int(minX, minY);

            int gridW = maxX - minX + 1;
            int gridH = maxY - minY + 1;

            // 패널이 없으면 200px 기준으로 fallback
            float panelSide = _panel != null
                ? Mathf.Min(_panel.rect.width, _panel.rect.height)
                : 200f;

            float available = panelSide - _padding * 2f;
            _step = available / Mathf.Max(gridW, gridH);
            _cellSize = _step * (1f - _gapRatio);

            // 격자 전체를 컨테이너 중앙에 맞춤
            _centerOffset = new Vector2(
                -(gridW - 1) * _step * 0.5f,
                -(gridH - 1) * _step * 0.5f
            );
        }

        private void CreateRoomCells(RoomMinimapEntry entry)
        {
            if (_cellPrefab == null) return;

            var cells = new List<MinimapCellUI>();

            foreach (var offset in GetCells(entry))
            {
                var cell = Instantiate(_cellPrefab, _cellContainer);
                var rt = cell.GetComponent<RectTransform>();
                rt.anchoredPosition = GridToLocal(entry.anchor + offset);
                rt.sizeDelta = new Vector2(_cellSize, _cellSize);
                cell.Initialize(entry.room);
                cells.Add(cell);
            }

            _roomCells[entry.room] = cells;
        }

        private void CreateAllConnectionLines()
        {
            if (_linePrefab == null) return;

            var drawn = new HashSet<(int, int)>();

            foreach (var entry in _layout.GetAllEntries())
            {
                if (entry.room?.doors == null) continue;

                foreach (var door in entry.room.doors)
                {
                    if (door.targetRoom == null) continue;

                    int idA = entry.room.GetInstanceID();
                    int idB = door.targetRoom.GetInstanceID();
                    var key = idA < idB ? (idA, idB) : (idB, idA);
                    if (!drawn.Add(key)) continue;

                    var targetEntry = _layout.GetEntry(door.targetRoom);
                    if (targetEntry == null) continue;

                    CreateLine(entry.anchor, targetEntry.anchor);
                }
            }
        }

        private void CreateLine(Vector2Int from, Vector2Int to)
        {
            var line = Instantiate(_linePrefab, _cellContainer);
            line.transform.SetAsFirstSibling();

            var posA = GridToLocal(from);
            var posB = GridToLocal(to);
            var dir = posB - posA;

            var rt = line.GetComponent<RectTransform>();
            rt.anchoredPosition = (posA + posB) * 0.5f;
            rt.sizeDelta = new Vector2(dir.magnitude, 2f);
            rt.localRotation = Quaternion.FromToRotation(Vector2.right, dir.normalized);
            line.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        }

        // ──────────────────────────────────────────────
        // Refresh
        // ──────────────────────────────────────────────

        private void Refresh()
        {
            if (MinimapManager.Instance == null) return;

            var current = MinimapManager.Instance.CurrentRoom;

            foreach (var (roomData, cells) in _roomCells)
            {
                MinimapManager.Instance.TryGetState(roomData, out var state);
                bool isVisited = state?.IsVisited ?? false;
                bool isCurrent = roomData == current;

                foreach (var cell in cells)
                    cell.UpdateState(isVisited, isCurrent);
            }
        }

        // ──────────────────────────────────────────────
        // Utility
        // ──────────────────────────────────────────────

        private Vector2 GridToLocal(Vector2Int grid)
        {
            var shifted = grid - _gridMin;
            return _centerOffset + new Vector2(shifted.x * _step, shifted.y * _step);
        }

        private static Vector2Int[] GetCells(RoomMinimapEntry entry)
            => (entry.cells != null && entry.cells.Length > 0)
                ? entry.cells
                : new[] { Vector2Int.zero };

        private void ToggleVisibility()
        {
            if (_canvasGroup == null) return;
            bool visible = _canvasGroup.alpha > 0.5f;
            _canvasGroup.alpha = visible ? 0f : 1f;
            _canvasGroup.blocksRaycasts = !visible;
        }
    }
}
