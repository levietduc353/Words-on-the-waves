using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    public class StaticCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _staticPanel;
        [SerializeField] private Button _storeButton;
        [SerializeField] private Button _mapButton;

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Awake()
        {
            if (_staticPanel != null)
            {
                _staticPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);
            }

            if (_storeButton != null)
                _storeButton.onClick.AddListener(OnStoreButtonClicked);
                
            if (_mapButton != null)
                _mapButton.onClick.AddListener(OnMapButtonClicked);
        }

        private void HandleGameStateChanged(string stateName)
        {
            if (_staticPanel == null) return;
            
            // Static UI hiện lên ở các state chơi chính (Service, Preparation, Cargo...)
            // Ẩn đi khi ở Map, MainMenu.
            bool isActiveState = stateName == "ServiceState" || 
                                 stateName == "PreparationState" || 
                                 stateName == "CargoState";
            _staticPanel.SetActive(isActiveState);
        }

        private void OnStoreButtonClicked()
        {
            // TODO: Phát sự kiện mở Cửa hàng
        }

        private void OnMapButtonClicked()
        {
            // TODO: Phát sự kiện mở lại bản đồ
        }
    }
}
