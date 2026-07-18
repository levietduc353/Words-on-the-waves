using UnityEngine;

namespace WordsOnTheWaves.Input
{
    /// <summary>
    /// Gắn lên bất kỳ GameObject nào. Kéo method Toggle() vào sự kiện
    /// OnClick() của Button trong Inspector để dùng.
    /// Mỗi lần nhấn sẽ bật/tắt GameObject được chỉ định.
    /// </summary>
    public class ToggleObjectButton : MonoBehaviour
    {
        [Tooltip("GameObject sẽ được bật/tắt khi nhấn nút này.")]
        [SerializeField] private GameObject _targetObject;

        /// <summary>
        /// Gán method này vào OnClick() của Button trong Inspector.
        /// Đổi trạng thái active của Target Object sang ngược lại.
        /// </summary>
        public void Toggle()
        {
            if (_targetObject == null)
            {
                Debug.LogWarning($"[ToggleObjectButton] Target Object chưa được gán trên '{gameObject.name}'.");
                return;
            }

            _targetObject.SetActive(!_targetObject.activeSelf);
        }
    }
}
