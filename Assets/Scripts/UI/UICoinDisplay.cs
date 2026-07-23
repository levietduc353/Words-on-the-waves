using UnityEngine;
using TMPro;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UICoinDisplay : MonoBehaviour
    {
        private TMP_Text _coinText;

        private void Awake()
        {
            _coinText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            EventManager.OnMoneyUpdated += UpdateCoinDisplay;
            
            // Khởi tạo hiển thị số tiền ban đầu
            if (DataManager.Instance != null)
            {
                UpdateCoinDisplay(DataManager.Instance.CurrentCoins);
            }
        }

        private void OnDisable()
        {
            EventManager.OnMoneyUpdated -= UpdateCoinDisplay;
        }

        private void UpdateCoinDisplay(int newAmount)
        {
            if (_coinText != null)
            {
                _coinText.text = newAmount.ToString();
            }
        }
    }
}
