using UnityEngine;

namespace WordsOnTheWaves.Preparation
{
    /// <summary>
    /// Gắn lên mỗi Empty GameObject làm slot trên kệ sách trong Editor.
    /// Lưu trạng thái slot (trống/có sách), hỗ trợ highlight khi kéo sách đến gần.
    /// </summary>
    public class BookSlot : MonoBehaviour
    {
        [Tooltip("Child object dùng làm visual indicator khi hover (bật/tắt).")]
        [SerializeField] private GameObject _highlightObject;

        private bool _isOccupied;
        private GameObject _placedBookObj;

        public bool IsOccupied => _isOccupied;

        private void Awake()
        {
            // Tắt highlight mặc định
            if (_highlightObject != null)
                _highlightObject.SetActive(false);
        }

        /// <summary>
        /// Bật/tắt visual highlight khi ghost book đang ở gần slot này.
        /// </summary>
        public void Highlight(bool active)
        {
            if (_highlightObject != null)
                _highlightObject.SetActive(active);
        }

        /// <summary>
        /// Đăng ký book object vừa được đặt vào slot này.
        /// </summary>
        public void PlaceBook(GameObject bookObj)
        {
            _placedBookObj = bookObj;
            _isOccupied    = true;
        }

        /// <summary>
        /// Xóa sách khỏi slot. Trả về bookId của cuốn sách vừa nhấc lên.
        /// Không Destroy object — người gọi chịu trách nhiệm.
        /// </summary>
        public string ReleaseBook()
        {
            string bookId = GetBookId();
            _placedBookObj = null;
            _isOccupied    = false;
            return bookId;
        }

        /// <summary>
        /// Lấy bookId của cuốn sách đang nằm trên slot (nếu có) mà không xóa nó.
        /// </summary>
        public string GetBookId()
        {
            if (_placedBookObj != null)
            {
                var pb = _placedBookObj.GetComponent<PlacedBook>();
                if (pb != null) return pb.BookId;
            }
            return null;
        }
    }
}
