using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Preparation;

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

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnEnable()
        {
            EventManager.OnBookPlaced         += OnBookPlaced;
            EventManager.OnBookPickedFromSlot += OnBookPickedFromSlot;
        }

        private void OnDisable()
        {
            EventManager.OnBookPlaced         -= OnBookPlaced;
            EventManager.OnBookPickedFromSlot -= OnBookPickedFromSlot;
        }

        private void OnClicked()
        {
            if (BookDragController.Instance == null) return;

            var prefab = GetRandomPrefab();
            if (prefab == null)
            {
                Debug.LogWarning($"[UIBookButton] Danh sách prefab trống cho bookId='{_bookId}'.");
                return;
            }

            BookDragController.Instance.BeginDragFromUI(_bookId, _bookGenre, prefab);
        }

        private GameObject GetRandomPrefab()
        {
            if (_bookPrefabs == null || _bookPrefabs.Count == 0) return null;
            return _bookPrefabs[Random.Range(0, _bookPrefabs.Count)];
        }

        // Sách đã đặt thành công lên kệ → ẩn button
        private void OnBookPlaced(string bookId)
        {
            if (bookId == _bookId)
                gameObject.SetActive(false);
        }

        // Sách được nhấc khỏi kệ trả về UI → hiện lại button
        private void OnBookPickedFromSlot(string bookId)
        {
            if (bookId == _bookId)
                gameObject.SetActive(true);
        }
    }
}
