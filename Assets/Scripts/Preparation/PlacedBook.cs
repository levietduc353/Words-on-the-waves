using UnityEngine;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Preparation
{
    /// <summary>
    /// Gắn lên prefab book 3D. Lưu thông tin cuốn sách đang chiếm slot này.
    /// Cần Collider để BookDragController phát hiện qua Raycast khi player click.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlacedBook : MonoBehaviour
    {
        public string    BookId     { get; private set; }
        public string    InstanceId { get; private set; }
        public BookGenre BookGenre  { get; private set; }
        public BookSlot  OwnerSlot  { get; private set; }

        /// <summary>Prefab gốc của cuốn sách này — dùng lại khi nhặt từ kệ.</summary>
        public GameObject Prefab    { get; private set; }

        /// <summary>
        /// Khởi tạo dữ liệu sau khi Instantiate.
        /// Gọi bởi BookDragController.ConfirmDrop() và CancelDrag().
        /// </summary>
        public void Init(string bookId, string instanceId, BookGenre bookGenre, BookSlot ownerSlot, GameObject prefab)
        {
            BookId     = bookId;
            InstanceId = instanceId;
            BookGenre  = bookGenre;
            OwnerSlot  = ownerSlot;
            Prefab     = prefab;
        }
    }
}
