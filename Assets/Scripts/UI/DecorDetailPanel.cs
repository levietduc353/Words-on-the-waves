using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Panel chi tiết dùng chung cho toàn bộ inventory. Khi một DecorSlotUI
    /// được nhấn, panel này sẽ hiện lên và điền thông tin của decor item đó.
    /// Tắt mặc định trong Scene, bật lên khi gọi Show().
    /// </summary>
    public class DecorDetailPanel : MonoBehaviour
    {
        public static DecorDetailPanel Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Image      _itemImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _buffText;
        [SerializeField] private Button     _closeButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_panel != null) _panel.SetActive(false);
        }

        private void Start()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
        }

        /// <summary>
        /// Hiển thị panel với thông tin của decor item tương ứng.
        /// </summary>
        public void Show(string decorId)
        {
            if (!DataManager.Instance.DecorItems.TryGetValue(decorId, out var data))
            {
                Debug.LogError($"[DecorDetailPanel] Không tìm thấy decor '{decorId}' trong config.");
                return;
            }

            // Điền hình ảnh lớn
            var sprite = Resources.Load<Sprite>($"Sprites/Decor/{decorId}");
            if (_itemImage != null)
            {
                _itemImage.sprite  = sprite;
                _itemImage.enabled = sprite != null;
            }

            // Điền tên item
            if (_nameText != null)
                _nameText.text = data.displayName;

            // Điền thông tin buff (ví dụ: "PriceBonus +5%")
            if (_buffText != null)
                _buffText.text = $"{data.buffType} +{data.buffPercent}%";

            if (_panel != null) _panel.SetActive(true);
        }

        /// <summary>
        /// Ẩn panel chi tiết.
        /// </summary>
        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }
    }
}
