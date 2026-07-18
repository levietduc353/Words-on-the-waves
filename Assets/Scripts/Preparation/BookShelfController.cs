using UnityEngine;

namespace WordsOnTheWaves.Preparation
{
    /// <summary>
    /// Gắn lên mỗi GameObject kệ sách (Bookshelf_L, Bookshelf_R).
    /// Tự thu thập tất cả BookSlot con qua GetComponentsInChildren khi Awake.
    /// </summary>
    public class BookShelfController : MonoBehaviour
    {
        // Tự điền bằng GetComponentsInChildren — không cần gán trong Inspector
        private BookSlot[] _slots;

        private void Awake()
        {
            _slots = GetComponentsInChildren<BookSlot>();
            Debug.Log($"[BookShelfController] '{gameObject.name}' thu thập được {_slots.Length} slot.");
        }

        /// <summary>
        /// Tìm slot trống gần nhất với vị trí worldPos.
        /// Trả về (null, float.MaxValue) nếu không có slot trống nào.
        /// </summary>
        public (BookSlot slot, float distance) GetNearestEmptySlot(Vector3 worldPos)
        {
            BookSlot nearest  = null;
            float    minDist  = float.MaxValue;

            foreach (var slot in _slots)
            {
                // Bỏ qua slot đã có sách
                if (slot.IsOccupied) continue;

                float dist = Vector3.Distance(worldPos, slot.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = slot;
                }
            }

            return (nearest, minDist);
        }

        /// <summary>
        /// Tìm slot trống gần nhất với mouseScreenPos (pixel). Dùng screen-space để
        /// hoạt động đúng với camera nhìn thẳng vào kệ sách nhiều tầng.
        /// </summary>
        public (BookSlot slot, float screenDist) GetNearestEmptySlotByScreen(
            Vector2 mouseScreenPos, Camera cam)
        {
            BookSlot nearest = null;
            float    minDist = float.MaxValue;

            foreach (var slot in _slots)
            {
                if (slot.IsOccupied) continue;

                Vector3 slotScreen = cam.WorldToScreenPoint(slot.transform.position);
                // Bỏ qua slot nằm sau lưng camera
                if (slotScreen.z < 0f) continue;

                float dist = Vector2.Distance(mouseScreenPos,
                                              new Vector2(slotScreen.x, slotScreen.y));
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = slot;
                }
            }

            return (nearest, minDist);
        }
    }
}
