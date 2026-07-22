using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.Preparation;

namespace WordsOnTheWaves.UI
{
    public class PrepareCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _preparePanel;
        [SerializeField] private Button     _confirmButton;

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
                _preparePanel.SetActive(false);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        private void OnConfirmButtonClicked()
        {
            string sceneToLoad = GameManager.Instance.TargetSceneToLoad;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                // Lấy dữ liệu sách trên kệ
                var cargoData = BookShelfManager.Instance.GetPlacedBooksCount();
                
                // Gửi event để DataManager lưu trữ thông tin mang sang scene mới
                EventManager.OnPreparationConfirmed?.Invoke(cargoData);

                // Chuyển scene
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
            _preparePanel.SetActive(stateName == "PreparationState");
        }
    }
}
