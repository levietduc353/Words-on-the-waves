using UnityEngine;
using TMPro;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    public class DynamicCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _dynamicPanel;
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private TextMeshProUGUI _timeText;

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChanged;
            EventManager.OnMoneyUpdated += UpdateMoneyDisplay;
            EventManager.OnTimeUpdated += UpdateTimeDisplay;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
            EventManager.OnMoneyUpdated -= UpdateMoneyDisplay;
            EventManager.OnTimeUpdated -= UpdateTimeDisplay;
        }

        private void Awake()
        {
            if (_dynamicPanel != null)
            {
                _dynamicPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);
            }

            // Set giá trị mặc định ban đầu
            if (_moneyText != null) _moneyText.text = "0";
            if (_timeText != null) _timeText.text = "08:00 AM";
        }

        private void HandleGameStateChanged(string stateName)
        {
            if (_dynamicPanel == null) return;

            // Thanh động hiện lên ở các state chơi chính
            bool isActiveState = stateName == "ServiceState" || 
                                 stateName == "PreparationState" || 
                                 stateName == "CargoState";
            _dynamicPanel.SetActive(isActiveState);
        }

        private void UpdateMoneyDisplay(int newAmount)
        {
            if (_moneyText != null)
            {
                _moneyText.text = newAmount.ToString();
            }
        }

        private void UpdateTimeDisplay(float newTime)
        {
            // Logic format time sẽ được cập nhật sau
        }
    }
}
