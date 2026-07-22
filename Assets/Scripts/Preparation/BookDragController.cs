using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.Preparation
{
    /// <summary>
    /// Singleton điều phối toàn bộ cơ chế kéo-đặt sách trong PreparationState.
    ///
    /// MECHANIC:
    ///   - Click UIBookButton → ghost book đi theo chuột (click-to-pick)
    ///   - Di chuyển chuột gần slot → ghost snap về slot (preview)
    ///   - Click lần 2 ở gần slot → đặt sách / click xa → hủy
    ///   - Click vào sách 3D trên kệ → nhặt lên di chuyển
    ///   - Click UI book khác khi đang giữ → đổi ghost
    /// </summary>
    public class BookDragController : MonoBehaviour
    {
        public static BookDragController Instance { get; private set; }

        [Header("Drag Settings")]
        [Tooltip("Y-coordinate của mặt phẳng nằm ngang dùng để tính vị trí chuột trong 3D.")]
        [SerializeField] private float _dragPlaneY = 0f;

        [Tooltip("Khoảng cách tối đa tính bằng pixel màn hình để ghost snap về slot.")]
        [SerializeField] private float _snapScreenPixels = 120f;

        [Tooltip("Rotation bù khi spawn sách (ghost và placed). Chỉnh để gáy sách hướng đúng.")]
        [SerializeField] private Vector3 _bookRotationOffset = new Vector3(-90f, 0f, 0f);

        // ==========================================
        // DRAG STATE
        // ==========================================

        private bool      _isDragging;
        private string    _draggingBookId;
        private string    _draggingInstanceId;
        private BookGenre _draggingGenre;
        private GameObject _draggingPrefab;

        /// <summary>Slot nguồn nếu drag từ kệ sách, null nếu từ UI.</summary>
        private BookSlot _sourceSlot;

        /// <summary>Slot đang được highlight để đặt sách vào.</summary>
        private BookSlot _currentTargetSlot;

        /// <summary>Ghost object đang đi theo chuột.</summary>
        private GameObject _ghostBook;

        private Plane _dragPlane;

        // ==========================================
        // UNITY LIFECYCLE
        // ==========================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (_isDragging)
            {
                UpdateGhostPosition();
                UpdateSnapPreview();
                HandleClickWhileDragging();
            }
            else
            {
                HandleClickWhileIdle();
            }
        }

        // ==========================================
        // PUBLIC API
        // ==========================================

        /// <summary>
        /// Bắt đầu drag từ UIBookButton. Gọi bởi UIBookButton.OnClick().
        /// Nếu đang drag rồi, thay ghost bằng sách mới.
        /// </summary>
        public void BeginDragFromUI(string bookId, string instanceId, BookGenre genre, GameObject prefab)
        {
            // Nếu cùng cuốn sách đang giữ → hủy (toggle off)
            if (_isDragging && _draggingBookId == bookId && _sourceSlot == null)
            {
                CancelDrag();
                return;
            }

            if (_isDragging) ClearDragState(destroyGhost: true);

            _sourceSlot = null;
            BeginDragInternal(bookId, instanceId, genre, prefab);
        }

        /// <summary>
        /// Bắt đầu drag từ sách đã đặt trên kệ.
        /// Gọi nội bộ bởi HandleClickWhileIdle() khi Raycast trúng PlacedBook.
        /// </summary>
        private void BeginDragFromSlot(PlacedBook placedBook)
        {
            if (_isDragging) ClearDragState(destroyGhost: true);

            _sourceSlot = placedBook.OwnerSlot;
            _sourceSlot.ReleaseBook();

            var prefab = placedBook.Prefab;
            BeginDragInternal(placedBook.BookId, placedBook.InstanceId, placedBook.BookGenre, prefab);

            // Destroy object cũ, ghost sẽ đại diện
            Destroy(placedBook.gameObject);

            EventManager.OnBookPickedFromSlot?.Invoke(placedBook.BookId, placedBook.InstanceId);
        }

        // ==========================================
        // PRIVATE LOGIC
        // ==========================================

        private void BeginDragInternal(string bookId, string instanceId, BookGenre genre, GameObject prefab)
        {
            _draggingBookId = bookId;
            _draggingInstanceId = instanceId;
            _draggingGenre  = genre;
            _draggingPrefab = prefab;
            _isDragging     = true;
            _dragPlane      = new Plane(Vector3.up, new Vector3(0f, _dragPlaneY, 0f));

            if (prefab != null)
            {
                var spawnPos = GetMouseWorldPos();

                // Giữ đúng rotation gốc của prefab thay vì Quaternion.identity
                _ghostBook = Instantiate(prefab, spawnPos, GetSpawnRotation(prefab));

                // Tắt Collider — tránh Raycast nhầm ghost
                foreach (var col in _ghostBook.GetComponentsInChildren<Collider>())
                    col.enabled = false;

                // Tắt PlacedBook — tránh xử lý nhầm
                var pb = _ghostBook.GetComponent<PlacedBook>();
                if (pb != null) pb.enabled = false;

                // Rigidbody: bắt buộc kinematic + tắt gravity
                // → ghost sẽ không bị physics ảnh hưởng, không xưyên collider
                var rb = _ghostBook.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity  = false;
                }
            }
            else
            {
                Debug.LogWarning($"[BookDragController] Prefab null cho bookId={bookId}.");
            }
        }

        /// <summary>Xử lý click khi đang giữ sách.</summary>
        private void HandleClickWhileDragging()
        {
            if (!IsMousePressedThisFrame()) return;

            // Nếu click trúng UI → UIBookButton tự xử lý (BeginDragFromUI)
            if (IsPointerOverUI()) return;

            // Nếu click trúng PlacedBook khác → đổi sang sách đó
            if (TryGetPlacedBookUnderCursor(out var pb))
            {
                BeginDragFromSlot(pb);
                return;
            }

            // Click vào 3D world → đặt hoặc hủy
            if (_currentTargetSlot != null)
                ConfirmDrop();
            else
                CancelDrag();
        }

        /// <summary>Xử lý click khi không giữ sách (nhặt sách trên kệ).</summary>
        private void HandleClickWhileIdle()
        {
            if (!IsMousePressedThisFrame()) return;
            if (IsPointerOverUI()) return;

            if (TryGetPlacedBookUnderCursor(out var pb))
                BeginDragFromSlot(pb);
        }

        private void UpdateGhostPosition()
        {
            if (_ghostBook == null) return;

            // Khi đang snap, ghost giữ nguyên tại slot position
            if (_currentTargetSlot != null)
            {
                _ghostBook.transform.position = _currentTargetSlot.transform.position;
                return;
            }

            _ghostBook.transform.position = GetMouseWorldPos();
        }

        private void UpdateSnapPreview()
        {
            Vector2 mouseScreen = GetMouseScreenPos();
            var cam = Camera.main;

            var (nearestSlot, screenDist) =
                BookShelfManager.Instance.GetNearestEmptySlotByScreen(mouseScreen, cam);

            // Tắt highlight slot cũ
            if (_currentTargetSlot != null && _currentTargetSlot != nearestSlot)
                _currentTargetSlot.Highlight(false);

            if (nearestSlot != null && screenDist <= _snapScreenPixels)
            {
                _currentTargetSlot = nearestSlot;
                _currentTargetSlot.Highlight(true);
            }
            else
            {
                _currentTargetSlot = null;
            }
        }

        /// <summary>Đặt sách vào target slot sau khi player click xác nhận.</summary>
        private void ConfirmDrop()
        {
            _currentTargetSlot.Highlight(false);

            if (_ghostBook != null) Destroy(_ghostBook);

            if (_draggingPrefab != null)
            {
                // Giữ rotation gốc của prefab khi đặt vào slot
                var bookObj = Instantiate(
                    _draggingPrefab,
                    _currentTargetSlot.transform.position,
                    GetSpawnRotation(_draggingPrefab));

                // Bắt buộc bật collider — phòng trường hợp prefab được lưu với collider tắt
                foreach (var col in bookObj.GetComponentsInChildren<Collider>(true))
                    col.enabled = true;

                var placedBook = bookObj.GetComponent<PlacedBook>();
                if (placedBook != null)
                    placedBook.Init(_draggingBookId, _draggingInstanceId, _draggingGenre, _currentTargetSlot, _draggingPrefab);

                _currentTargetSlot.PlaceBook(bookObj);
            }

            if (_sourceSlot == null)
            {
                WordsOnTheWaves.Core.DataManager.Instance.RemoveBooks(_draggingBookId, 1);
                EventManager.OnBookPlaced?.Invoke(_draggingBookId, _draggingInstanceId);
            }

            ClearDragState(destroyGhost: false);
        }

        /// <summary>Hủy drag: trả sách về kho nếu từ kệ, UI tự lo nếu từ button.</summary>
        private void CancelDrag()
        {
            if (_currentTargetSlot != null)
            {
                _currentTargetSlot.Highlight(false);
                _currentTargetSlot = null;
            }

            // Nếu drag từ slot → thay vì khôi phục lại kệ, hoàn trả sách về kho
            if (_sourceSlot != null && _draggingPrefab != null)
            {
                WordsOnTheWaves.Core.DataManager.Instance.AddBooks(_draggingBookId, 1);
                EventManager.OnBookReturnedToInventory?.Invoke(_draggingBookId);
            }
            // Nếu drag từ UI → không cần phát event, sách chưa bị trừ trong kho

            ClearDragState(destroyGhost: true);
        }

        private void ClearDragState(bool destroyGhost)
        {
            if (destroyGhost && _ghostBook != null)
                Destroy(_ghostBook);

            _isDragging        = false;
            _ghostBook         = null;
            _draggingBookId    = null;
            _draggingInstanceId = null;
            _draggingPrefab    = null;
            _sourceSlot        = null;
            _currentTargetSlot = null;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private bool TryGetPlacedBookUnderCursor(out PlacedBook placedBook)
        {
            placedBook = null;
            var ray = Camera.main.ScreenPointToRay(GetMouseScreenPos());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                placedBook = hit.collider.GetComponent<PlacedBook>();
            }
            return placedBook != null;
        }

        private Vector3 GetMouseWorldPos()
        {
            var ray = Camera.main.ScreenPointToRay(GetMouseScreenPos());
            if (_dragPlane.Raycast(ray, out float t))
                return ray.GetPoint(t);
            return Vector3.zero;
        }

        private bool IsMousePressedThisFrame()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        private Vector2 GetMouseScreenPos()
        {
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            return Vector2.zero;
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Tính rotation cuối cùng = rotation gốc của prefab + offset điều chỉnh.
        /// </summary>
        private Quaternion GetSpawnRotation(GameObject prefab)
        {
            return Quaternion.Euler(_bookRotationOffset) * prefab.transform.rotation;
        }

    }
}
