using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.UI
{
    [System.Serializable]
    public struct CrateButtonConfig
    {
        [Tooltip("Nhập ID của Crate vào đây (VD: crate_light, crate_mystery)")]
        public string crateId;
        public Button button;
    }

    [System.Serializable]
    public struct GenreTextConfig
    {
        [Tooltip("Thể loại sách mà Text này sẽ hiển thị")]
        public BookGenre genre;
        public TMP_Text resultText;
    }

    public class StoreCanvasController : MonoBehaviour
    {
        [Header("Shop Settings")]
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private CrateButtonConfig[] _crateButtons;
        
        [Header("Unbox Result Panel")]
        [SerializeField] private GameObject _unboxResultPanel;
        [SerializeField] private GenreTextConfig[] _genreResultTexts;

        private void Awake()
        {
            if (_errorText != null) _errorText.gameObject.SetActive(false);
            if (_unboxResultPanel != null) _unboxResultPanel.SetActive(false);

            // Gán sự kiện click cho các nút crate theo cấu hình
            if (_crateButtons != null)
            {
                foreach (var config in _crateButtons)
                {
                    if (config.button != null && !string.IsNullOrEmpty(config.crateId))
                    {
                        string targetId = config.crateId;
                        config.button.onClick.AddListener(() => OnBuyCrateClicked(targetId));
                    }
                }
            }
        }

        private void OnBuyCrateClicked(string crateId)
        {
            if (!DataManager.Instance.Crates.TryGetValue(crateId, out var crateData))
            {
                Debug.LogError($"[StoreCanvas] Không tìm thấy Crate ID: {crateId}");
                return;
            }

            // Kiểm tra và trừ tiền
            if (!DataManager.Instance.TrySpendCoins(crateData.cratePrice))
            {
                if (_errorText != null)
                {
                    _errorText.text = "You don't have enough money";
                    _errorText.gameObject.SetActive(true);
                    CancelInvoke(nameof(HideErrorText));
                    Invoke(nameof(HideErrorText), 3f);
                }
                return;
            }

            // Mở hòm nếu đủ tiền
            UnboxCrate(crateData);
        }

        private void UnboxCrate(CrateData crateData)
        {
            // Reset text về 0
            if (_genreResultTexts != null)
            {
                foreach (var config in _genreResultTexts)
                {
                    if (config.resultText != null)
                        config.resultText.text = "0";
                }
            }

            // Tiến hành Random
            Dictionary<BookGenre, int> unboxResults = new Dictionary<BookGenre, int>();

            if (crateData.genreSlots != null)
            {
                foreach (var slot in crateData.genreSlots)
                {
                    // +1 vì Random.Range cho int bị exclusive giá trị max
                    int amount = Random.Range(slot.dropMin, slot.dropMax + 1); 
                    if (amount > 0)
                    {
                        if (!unboxResults.ContainsKey(slot.genre))
                            unboxResults[slot.genre] = 0;
                        unboxResults[slot.genre] += amount;

                        // Tìm danh sách ID sách thuộc thể loại này
                        var booksOfGenre = DataManager.Instance.GetBooksByGenre(slot.genre);
                        if (booksOfGenre.Count > 0)
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                // Chọn ngẫu nhiên 1 cuốn trong thể loại đó và cộng vào kho
                                var randomBook = booksOfGenre[Random.Range(0, booksOfGenre.Count)];
                                DataManager.Instance.AddBooks(randomBook.id, 1);
                            }
                        }
                    }
                }
            }

            // Hiển thị kết quả lên giao diện (chỉ hiện số)
            if (_genreResultTexts != null && unboxResults.Count > 0)
            {
                foreach (var kvp in unboxResults)
                {
                    BookGenre genre = kvp.Key;
                    int amount = kvp.Value;
                    
                    // Tìm cấu hình hiển thị text cho genre này
                    foreach (var config in _genreResultTexts)
                    {
                        if (config.genre == genre && config.resultText != null)
                        {
                            config.resultText.text = amount.ToString();
                            break;
                        }
                    }
                }
            }

            // Bật màn hình kết quả lên
            if (_unboxResultPanel != null)
            {
                _unboxResultPanel.SetActive(true);
            }
        }

        private void HideErrorText()
        {
            if (_errorText != null) _errorText.gameObject.SetActive(false);
        }
    }
}
