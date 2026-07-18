using UnityEngine;

namespace WordsOnTheWaves.Preparation
{
    /// <summary>
    /// Singleton quản lý cả 2 BookShelfController trong scene.
    /// Cung cấp API duy nhất GetNearestEmptySlot() để BookDragController
    /// tìm slot gần nhất xuyên suốt cả 2 kệ sách.
    /// </summary>
    public class BookShelfManager : MonoBehaviour
    {
        public static BookShelfManager Instance { get; private set; }

        [Tooltip("Kéo cả 2 BookShelfController (Bookshelf_L và Bookshelf_R) vào đây.")]
        [SerializeField] private BookShelfController[] _shelves;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Tìm slot trống gần nhất với worldPos, duyệt qua cả 2 kệ sách.
        /// Trả về (null, float.MaxValue) nếu toàn bộ slot đều bị chiếm.
        /// </summary>
        public (BookSlot slot, float distance) GetNearestEmptySlot(Vector3 worldPos)
        {
            BookSlot nearest = null;
            float    minDist = float.MaxValue;

            foreach (var shelf in _shelves)
            {
                if (shelf == null) continue;

                var (slot, dist) = shelf.GetNearestEmptySlot(worldPos);
                if (slot != null && dist < minDist)
                {
                    minDist = dist;
                    nearest = slot;
                }
            }

            return (nearest, minDist);
        }

        /// <summary>
        /// Tìm slot trống gần nhất theo screen-space, duyệt cả 2 kệ.
        /// </summary>
        public (BookSlot slot, float screenDist) GetNearestEmptySlotByScreen(
            Vector2 mouseScreenPos, Camera cam)
        {
            BookSlot nearest = null;
            float    minDist = float.MaxValue;

            foreach (var shelf in _shelves)
            {
                if (shelf == null) continue;

                var (slot, dist) = shelf.GetNearestEmptySlotByScreen(mouseScreenPos, cam);
                if (slot != null && dist < minDist)
                {
                    minDist = dist;
                    nearest = slot;
                }
            }

            return (nearest, minDist);
        }
    }
}
