using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Preparation;
using WordsOnTheWaves.Core;
using TMPro;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Gắn lên mỗi Button sách trong Preparation UI.
    /// Config trực tiếp bookId, genre và prefab 3D ngay trên component trong Inspector.
    /// Khi click → BookDragController bắt đầu drag sách tương ứng.
    /// Tự ẩn khi sách được đặt thành công lên kệ (OnBookPlaced).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIBookButton : MonoBehaviour
    {
        [Header("Book Config")]
        [Tooltip("ID của cuốn sách (phải khớp với bookId trong books_config.json).")]
        [SerializeField] private string _bookId;

        [Tooltip("Thể loại sách.")]
        [SerializeField] private BookGenre _bookGenre;

        [Tooltip("Danh sách prefab 3D — mỗi lần click sẽ chọn ngẫu nhiên 1 cái.")]
        [SerializeField] private List<GameObject> _bookPrefabs = new List<GameObject>();

        [Header("UI Refs")]
        [Tooltip("Text hiển thị số lượng sách còn trong kho.")]
        [SerializeField] private TMP_Text _countText;

        private Button _button;
        private string _uiInstanceId;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _uiInstanceId = System.Guid.NewGuid().ToString();
        }

        private void OnEnable()
        {
            EventManager.OnInventoryChanged += HandleInventoryChanged;
            EventManager.OnDataLoaded += HandleDataLoaded;
            UpdateCountDisplay();
        }

        private void OnDisable()
        {
            EventManager.OnInventoryChanged -= HandleInventoryChanged;
            EventManager.OnDataLoaded -= HandleDataLoaded;
        }

        private void Start()
        {
            _button.onClick.AddListener(OnClicked);
            UpdateCountDisplay();
        }

        private void OnClicked()
        {
            if (BookDragController.Instance == null) return;
            if (DataManager.Instance == null) return;

            int currentCount = DataManager.Instance.GetBookCount(_bookId);
            if (currentCount <= 0)
            {
                Debug.Log($"[UIBookButton] Hết sách '{_bookId}' trong kho!");
                return;
            }

            var prefab = GetRandomPrefab();
            if (prefab == null)
            {
                Debug.LogWarning($"[UIBookButton] Danh sách prefab trống cho bookId='{_bookId}'.");
                return;
            }

            BookDragController.Instance.BeginDragFromUI(_bookId, _uiInstanceId, _bookGenre, prefab);
        }

        private GameObject GetRandomPrefab()
        {
            if (_bookPrefabs == null || _bookPrefabs.Count == 0) return null;
            return _bookPrefabs[Random.Range(0, _bookPrefabs.Count)];
        }

        private void HandleInventoryChanged(string bookId, int newAmount)
        {
            if (bookId == _bookId)
            {
                UpdateCountDisplay();
            }
        }

        private void HandleDataLoaded()
        {
            UpdateCountDisplay();
        }

        private void UpdateCountDisplay()
        {
            if (_countText == null || DataManager.Instance == null) return;
            
            int count = DataManager.Instance.GetBookCount(_bookId);
            _countText.text = count.ToString();
        }

    }
}
