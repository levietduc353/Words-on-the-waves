using UnityEngine;
using UnityEngine.UI;

namespace WordsOnTheWaves.UI
{
    [RequireComponent(typeof(Button))]
    public class DisableObjectOnClick : MonoBehaviour
    {
        [Tooltip("Object bạn muốn ẩn đi khi bấm nút này")]
        [SerializeField] private GameObject _targetObjectToDisable;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (_targetObjectToDisable != null)
            {
                _targetObjectToDisable.SetActive(false);
            }
            else
            {
                // Nếu chưa gán target object, mặc định sẽ ẩn chính game object đang chứa nút này
                gameObject.SetActive(false);
            }
        }
    }
}
