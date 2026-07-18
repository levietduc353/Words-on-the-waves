using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Gắn lên prefab DecorSlot. Mỗi slot đại diện cho một decor item
    /// trong inventory của player. Hiển thị sprite item, khi nhấn sẽ mở
    /// Detail Panel để xem thông tin chi tiết.
    /// </summary>
    public class DecorSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _itemImage;
        [SerializeField] private Button _button;

        private string _decorId;

        /// <summary>
        /// Khởi tạo slot với dữ liệu của decor item tương ứng.
        /// Sprite được load từ Resources/Sprites/Decor/{decorId}.
        /// </summary>
        public void Setup(string decorId)
        {
            _decorId = decorId;

            // Load sprite theo convention: tên file = decorId
            var sprite = Resources.Load<Sprite>($"Sprites/Decor/{decorId}");
            if (sprite != null)
            {
                _itemImage.sprite = sprite;
                _itemImage.enabled = true;
            }
            else
            {
                // Ẩn image nếu chưa có sprite, tránh hiện placeholder vỡ
                _itemImage.enabled = false;
                Debug.LogWarning($"[DecorSlotUI] Không tìm thấy sprite tại Resources/Sprites/Decor/{decorId}");
            }

            // Đăng ký sự kiện click
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            DecorDetailPanel.Instance.Show(_decorId);
        }
    }
}
