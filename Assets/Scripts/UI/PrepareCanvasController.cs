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
        [SerializeField] private Button     _backButton;
        [SerializeField] private TMPro.TMP_Text _errorText;

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
            if (_errorText != null)
                _errorText.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(() => EventManager.OnBackToMapClicked?.Invoke());
        }

        private void OnConfirmButtonClicked()
        {
            string sceneToLoad = GameManager.Instance.TargetSceneToLoad;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                int travelFee = DataManager.Instance.GetTravelFeeBySceneName(sceneToLoad);
                if (!DataManager.Instance.TrySpendCoins(travelFee))
                {
                    if (_errorText != null)
                    {
                        _errorText.text = "You don't have enough money";
                        _errorText.gameObject.SetActive(true);
                        CancelInvoke(nameof(HideErrorText));
                        Invoke(nameof(HideErrorText), 3f);
                    }
                    return; // Ngừng chuyển scene vì không đủ tiền
                }

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

        private void HideErrorText()
        {
            if (_errorText != null)
            {
                _errorText.gameObject.SetActive(false);
            }
        }

        private void HandleGameStateChanged(string stateName)
        {
            if (_preparePanel == null) return;
            _preparePanel.SetActive(stateName == "PreparationState");
        }
    }
}
