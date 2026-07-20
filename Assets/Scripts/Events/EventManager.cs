using System;
using UnityEngine;

namespace WordsOnTheWaves.Events
{
    public static class EventManager
    {
        // ==========================================
        // UI & STATE EVENTS
        // ==========================================
        
        /// <summary>
        /// Phát ra khi DataManager load xong toàn bộ JSON config.
        /// Các system cần data (UI, SpawnSystem...) nên lắng nghe event này trước khi init.
        /// </summary>
        public static Action OnDataLoaded;

        /// <summary>
        /// Phát ra khi số lượng sách trong kho thay đổi.
        /// Tham số: bookId (string), số lượng mới (int).
        /// </summary>
        public static Action<string, int> OnInventoryChanged;

        /// <summary>
        /// Phát ra khi người chơi ấn Start Game ở Main Menu
        /// </summary>
        public static Action OnStartGameClicked;

        /// <summary>
        /// Phát ra khi người chơi chọn một bãi biển trên Map. Tham số: Tên scene đích.
        /// </summary>
        public static Action<string> OnMapSelected;

        /// <summary>
        /// Phát ra khi người chơi nhấn nút Decor.
        /// </summary>
        public static Action OnDecorButtonClicked;

        /// <summary>
        /// Phát ra khi người chơi nhấn nút Back để từ giao diện khác quay lại Map.
        /// </summary>
        public static Action OnBackToMapClicked;

        /// <summary>
        /// Phát ra khi chuyển đổi GameState. Tham số là tên của State mới.
        /// </summary>
        public static Action<string> OnGameStateChanged;

        /// <summary>
        /// Phát ra khi danh sách decor trong inventory thay đổi.
        /// Tham số: decorId vừa được thêm vào.
        /// </summary>
        public static Action<string> OnDecorInventoryChanged;

        /// <summary>
        /// Phát ra khi player đặt thành công một cuốn sách lên kệ từ inventory UI.
        /// Tham số: bookId, instanceId (UI id để phân biệt các nút)
        /// </summary>
        public static Action<string, string> OnBookPlaced;

        /// <summary>
        /// Phát ra khi player nhấc một cuốn sách đang trên kệ xuống.
        /// Tham số: bookId, instanceId
        /// </summary>
        public static Action<string, string> OnBookPickedFromSlot;

        /// <summary>
        /// Phát ra khi cập nhật số tiền hiện tại. Tham số: Số tiền mới.
        /// </summary>
        public static Action<int> OnMoneyUpdated;

        /// <summary>
        /// Phát ra khi cập nhật thời gian. Tham số: Giá trị thời gian mới.
        /// </summary>
        public static Action<float> OnTimeUpdated;


        // ==========================================
        // INPUT EVENTS
        // ==========================================

        /// <summary>
        /// Phát ra khi người chơi click/tap trúng một vật thể 3D (sách, khách hàng, đồ trang trí).
        /// Tham số: GameObject bị click.
        /// </summary>
        public static Action<GameObject> OnInteractableTapped;
        
        /// <summary>
        /// Phát ra khi bắt đầu thao tác Drag (Kéo). Tham số: GameObject đang bị kéo.
        /// </summary>
        public static Action<GameObject> OnObjectDragStarted;

        /// <summary>
        /// Phát ra khi kết thúc thao tác Drop (Thả). Tham số: GameObject vừa thả.
        /// </summary>
        public static Action<GameObject> OnObjectDropped;

        // ==========================================
        // NPC EVENTS
        // ==========================================

        /// <summary>
        /// Phát ra khi NPC khách hàng đi đến điểm dừng (posStop).
        /// Tham số: GameObject của NPC.
        /// </summary>
        public static Action<GameObject> OnCustomerArrived;

        /// <summary>
        /// Phát ra khi NPC khách hàng đã đi khỏi tiệm (về posB và bị trả lại pool).
        /// Tham số: GameObject của NPC.
        /// </summary>
        public static Action<GameObject> OnCustomerLeft;
    }
}
