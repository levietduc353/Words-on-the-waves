using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.UI
{
    public class MainMenuCanvasController : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        private void Start()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartButtonClicked);
            }
        }

        private void OnStartButtonClicked()
        {
            EventManager.OnStartGameClicked?.Invoke();
        }
    }
}
