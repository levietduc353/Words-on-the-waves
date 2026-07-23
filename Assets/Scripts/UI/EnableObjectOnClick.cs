using UnityEngine;
using UnityEngine.UI;

namespace WordsOnTheWaves.UI
{
    [RequireComponent(typeof(Button))]
    public class EnableObjectOnClick : MonoBehaviour
    {
        [Tooltip("Object bạn muốn bật lên khi bấm nút này")]
        [SerializeField] private GameObject _targetObjectToEnable;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (_targetObjectToEnable != null)
            {
                _targetObjectToEnable.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[EnableObjectOnClick] Chưa gán đối tượng cần bật cho nút {gameObject.name}");
            }
        }
    }
}
