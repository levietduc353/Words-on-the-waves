using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    public class DecorCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _decorPanel;
        [SerializeField] private Button _backButton;

        private void Awake()
        {
            if (_decorPanel != null)
            {
                _decorPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(() => EventManager.OnBackToMapClicked?.Invoke());
            }
        }

        private void HandleGameStateChanged(string stateName)
        {
            if (_decorPanel == null) return;
            
            if (stateName == "DecorState")
            {
                _decorPanel.SetActive(true);
            }
            else
            {
                _decorPanel.SetActive(false);
            }
        }
    }
}
