using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    public class PrepareCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _preparePanel;
        [SerializeField] private Button _confirmButton;

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
            if (_preparePanel != null)
            {
                _preparePanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }
        }

        private void OnConfirmButtonClicked()
        {
            string sceneToLoad = GameManager.Instance.TargetSceneToLoad;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                // Chuyển sang scene bãi biển đã chọn từ map
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("[PrepareCanvas] No Target Scene found in GameManager!");
            }
        }

        private void HandleGameStateChanged(string stateName)
        {
            if (_preparePanel == null) return;

            // Chỉ hiện PrepareCanvas khi ở PreparationState
            if (stateName == "PreparationState")
            {
                _preparePanel.SetActive(true);
            }
            else
            {
                _preparePanel.SetActive(false);
            }
        }
    }
}
