using UnityEngine;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Script test: spawn một số slot trống vào lưới để kiểm tra layout.
    /// CHỈ DÙNG ĐỂ TEST — xóa hoặc disable trước khi build.
    /// </summary>
    public class DecorInventoryTestSpawner : MonoBehaviour
    {
        [Tooltip("Prefab của slot (phải có component DecorSlotUI).")]
        [SerializeField] private DecorSlotUI _slotPrefab;

        [Tooltip("Transform Content bên trong ScrollView (nơi có GridLayoutGroup).")]
        [SerializeField] private Transform _gridContent;

        [Tooltip("Số slot giả muốn spawn để test layout.")]
        [SerializeField] private int _spawnCount = 7;

        [ContextMenu("Spawn Test Slots")]
        private void Start()
        {
            SpawnTestSlots();
        }

        private void SpawnTestSlots()
        {
            if (_slotPrefab == null || _gridContent == null)
            {
                Debug.LogError("[TestSpawner] Chưa gán SlotPrefab hoặc GridContent trong Inspector.");
                return;
            }

            // Xóa slot cũ nếu có
            foreach (Transform child in _gridContent)
                Destroy(child.gameObject);

            // Spawn slot trống — không gọi Setup() nên không cần DataManager
            for (int i = 0; i < _spawnCount; i++)
                Instantiate(_slotPrefab, _gridContent);

            Debug.Log($"[TestSpawner] Đã spawn {_spawnCount} slot test.");
        }
    }
}
