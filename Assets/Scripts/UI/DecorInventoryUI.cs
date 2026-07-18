using UnityEngine;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Quản lý lưới hiển thị decor inventory. Gắn lên GameObject chứa ScrollView.
    /// Tự động spawn/refresh DecorSlotUI prefab mỗi khi inventory thay đổi.
    /// </summary>
    public class DecorInventoryUI : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Tooltip("Prefab của một ô slot trong lưới. Phải có component DecorSlotUI.")]
        [SerializeField] private DecorSlotUI _slotPrefab;

        [Tooltip("Transform Content bên trong ScrollView (nơi có GridLayoutGroup).")]
        [SerializeField] private Transform _gridContent;

        private void OnEnable()
        {
            EventManager.OnDataLoaded            += RefreshGrid;
            EventManager.OnDecorInventoryChanged += OnDecorAdded;
        }

        private void OnDisable()
        {
            EventManager.OnDataLoaded            -= RefreshGrid;
            EventManager.OnDecorInventoryChanged -= OnDecorAdded;
        }

        /// <summary>
        /// Được gọi khi DataManager load xong — khởi tạo lưới từ inventory hiện có.
        /// </summary>
        private void RefreshGrid()
        {
            // Xóa toàn bộ slot cũ
            foreach (Transform child in _gridContent)
                Destroy(child.gameObject);

            // Spawn lại từ đầu theo danh sách OwnedDecors
            var ownedDecors = DataManager.Instance.OwnedDecors;
            foreach (var decorId in ownedDecors)
                SpawnSlot(decorId);
        }

        /// <summary>
        /// Được gọi khi một decor mới được thêm vào inventory trong runtime.
        /// Chỉ spawn thêm slot mới, không rebuild cả lưới.
        /// </summary>
        private void OnDecorAdded(string decorId)
        {
            SpawnSlot(decorId);
        }

        /// <summary>
        /// Tạo một slot mới trong lưới và khởi tạo với decorId tương ứng.
        /// </summary>
        private void SpawnSlot(string decorId)
        {
            if (_slotPrefab == null || _gridContent == null)
            {
                Debug.LogError("[DecorInventoryUI] SlotPrefab hoặc GridContent chưa được gán trong Inspector.");
                return;
            }

            var slot = Instantiate(_slotPrefab, _gridContent);
            slot.Setup(decorId);
        }
    }
}
