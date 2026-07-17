using UnityEngine;
using UnityEngine.InputSystem;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.InputHandling
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _interactableLayer;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            // Tuân thủ Zero GC: Không cấp phát bộ nhớ mới trong Update
            // Sử dụng Input System mới
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                HandleTap(mousePosition);
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                HandleTap(touchPosition);
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _interactableLayer))
            {
                // Phát sự kiện chạm vào vật thể 3D (sách, khách, đồ trang trí)
                EventManager.OnInteractableTapped?.Invoke(hit.collider.gameObject);
            }
        }
    }
}
