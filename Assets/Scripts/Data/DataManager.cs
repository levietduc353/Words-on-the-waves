using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.Core
{
    /// <summary>
    /// Singleton tập trung load và cache toàn bộ JSON config của game.
    /// Các system khác truy cập dữ liệu thông qua DataManager.Instance (read-only).
    /// Phải tồn tại trước GameManager trong scene — đặt Script Execution Order ưu tiên cao hơn.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        // ==========================================
        // PUBLIC READ-ONLY DATA COLLECTIONS
        // ==========================================

        /// <summary>Tất cả sách, tra cứu theo book ID (O(1)).</summary>
        public IReadOnlyDictionary<string, BookData> Books => _books;

        /// <summary>Tất cả kiện hàng trong shop, tra cứu theo crate ID.</summary>
        public IReadOnlyDictionary<string, CrateData> Crates => _crates;

        /// <summary>Tất cả địa điểm bãi biển, tra cứu theo location ID.</summary>
        public IReadOnlyDictionary<string, LocationData> Locations => _locations;

        /// <summary>Tất cả vật trang trí, tra cứu theo decor ID.</summary>
        public IReadOnlyDictionary<string, DecorItemData> DecorItems => _decorItems;

        /// <summary>Tất cả kỷ vật, tra cứu theo memento ID.</summary>
        public IReadOnlyDictionary<string, MementoData> Mementos => _mementos;

        /// <summary>Toàn bộ dialogue của game.</summary>
        public DialogueData Dialogues => _dialogues;

        /// <summary>Cấu hình tiền tệ.</summary>
        public CoinConfig CoinConfig => _coinConfig;

        /// <summary>Danh sách quy tắc spawn khách hàng (dùng cho weighted random).</summary>
        public IReadOnlyList<CustomerSpawnRule> CustomerSpawnRules => _customerSpawnRules;

        /// <summary>Danh sách Special Customer với thông tin memento riêng.</summary>
        public IReadOnlyList<SpecialCustomerData> SpecialCustomers => _specialCustomers;

        // ==========================================
        // PRIVATE BACKING FIELDS
        // ==========================================

        private readonly Dictionary<string, BookData>      _books      = new Dictionary<string, BookData>();
        private readonly Dictionary<string, CrateData>     _crates     = new Dictionary<string, CrateData>();
        private readonly Dictionary<string, LocationData>  _locations  = new Dictionary<string, LocationData>();
        private readonly Dictionary<string, DecorItemData> _decorItems = new Dictionary<string, DecorItemData>();
        private readonly Dictionary<string, MementoData>   _mementos   = new Dictionary<string, MementoData>();

        private DialogueData           _dialogues;
        private CoinConfig             _coinConfig;
        private List<CustomerSpawnRule>    _customerSpawnRules = new List<CustomerSpawnRule>();
        private List<SpecialCustomerData>  _specialCustomers   = new List<SpecialCustomerData>();

        // Cache danh sách sách theo genre để GetBooksByGenre() không phải duyệt lại mỗi lần
        private readonly Dictionary<BookGenre, List<BookData>> _booksByGenre = new Dictionary<BookGenre, List<BookData>>();

        // Tồn kho sách hiện tại của người chơi (mutable — thay đổi trong gameplay)
        // Key: book id | Value: số lượng đang có
        private readonly Dictionary<string, int> _bookInventory = new Dictionary<string, int>();

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
            DontDestroyOnLoad(gameObject);

            LoadAllData();
        }

        // ==========================================
        // LOAD METHODS
        // ==========================================

        /// <summary>Load toàn bộ JSON config ngay khi khởi động.</summary>
        private void LoadAllData()
        {
            LoadBooks();
            LoadCrates();
            LoadLocations();
            LoadCustomers();
            LoadDecorItems();
            LoadMementos();
            LoadDialogues();
            LoadCoinConfig();
            LoadBookInventory();

            Debug.Log($"[DataManager] Loaded: {_books.Count} books | {_crates.Count} crates | " +
                      $"{_locations.Count} locations | {_decorItems.Count} decor | {_mementos.Count} mementos");

            // Thông báo cho các Observer rằng dữ liệu đã sẵn sàng
            EventManager.OnDataLoaded?.Invoke();
        }

        private void LoadBooks()
        {
            var text = LoadJson("Data/books_config");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<BookDataListWrapper>(text);
            if (wrapper?.books == null) return;

            foreach (var book in wrapper.books)
            {
                _books[book.id] = book;

                // Build cache theo genre
                if (!_booksByGenre.ContainsKey(book.genre))
                    _booksByGenre[book.genre] = new List<BookData>();
                _booksByGenre[book.genre].Add(book);
            }
        }

        private void LoadCrates()
        {
            var text = LoadJson("Data/crates_config");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<CrateDataListWrapper>(text);
            if (wrapper?.crates == null) return;

            foreach (var crate in wrapper.crates)
                _crates[crate.id] = crate;
        }

        private void LoadLocations()
        {
            var text = LoadJson("Data/locations_config");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<LocationDataListWrapper>(text);
            if (wrapper?.locations == null) return;

            foreach (var loc in wrapper.locations)
                _locations[loc.id] = loc;
        }

        private void LoadCustomers()
        {
            var text = LoadJson("Data/customers_config");
            if (text == null) return;

            var config = JsonConvert.DeserializeObject<CustomerConfig>(text);
            if (config == null) return;

            _customerSpawnRules = config.spawnRules ?? new List<CustomerSpawnRule>();
            _specialCustomers   = config.specialCustomers ?? new List<SpecialCustomerData>();
        }

        private void LoadDecorItems()
        {
            var text = LoadJson("Data/decor_config");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<DecorItemDataListWrapper>(text);
            if (wrapper?.decorItems == null) return;

            foreach (var item in wrapper.decorItems)
                _decorItems[item.id] = item;
        }

        private void LoadMementos()
        {
            var text = LoadJson("Data/memento_config");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<MementoDataListWrapper>(text);
            if (wrapper?.mementos == null) return;

            foreach (var memento in wrapper.mementos)
                _mementos[memento.id] = memento;
        }

        private void LoadDialogues()
        {
            var text = LoadJson("Data/dialogues_config");
            if (text == null) return;

            _dialogues = JsonConvert.DeserializeObject<DialogueData>(text);
        }

        private void LoadCoinConfig()
        {
            var text = LoadJson("Data/coins_config");
            if (text == null) return;

            _coinConfig = JsonConvert.DeserializeObject<CoinConfig>(text);
        }

        private void LoadBookInventory()
        {
            var text = LoadJson("Data/book_inv");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<BookInventoryWrapper>(text);
            if (wrapper?.inventory == null) return;

            foreach (var entry in wrapper.inventory)
                _bookInventory[entry.Key] = entry.Value;
        }

        /// <summary>Đọc TextAsset từ Resources, trả về null nếu không tìm thấy.</summary>
        private string LoadJson(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[DataManager] Cannot find JSON at Resources/{resourcePath}");
                return null;
            }
            return asset.text;
        }

        // ==========================================
        // HELPER METHODS (PUBLIC API)
        // ==========================================

        /// <summary>
        /// Lấy danh sách sách theo thể loại. Trả về danh sách rỗng nếu không có.
        /// </summary>
        public List<BookData> GetBooksByGenre(BookGenre genre)
        {
            return _booksByGenre.TryGetValue(genre, out var list) ? list : new List<BookData>();
        }

        /// <summary>
        /// Lấy số lượng sách đang có trong kho theo book ID.
        /// </summary>
        public int GetBookCount(string bookId)
        {
            return _bookInventory.TryGetValue(bookId, out int count) ? count : 0;
        }

        /// <summary>
        /// Thêm sách vào kho. Phát OnInventoryChanged sau khi cập nhật.
        /// </summary>
        public void AddBooks(string bookId, int amount)
        {
            if (amount <= 0) return;
            if (!_bookInventory.ContainsKey(bookId))
                _bookInventory[bookId] = 0;

            _bookInventory[bookId] += amount;
            EventManager.OnInventoryChanged?.Invoke(bookId, _bookInventory[bookId]);
        }

        /// <summary>
        /// Bỏ sách khỏi kho. Trả về false nếu không đủ số lượng.
        /// </summary>
        public bool RemoveBooks(string bookId, int amount)
        {
            if (amount <= 0) return false;
            if (!_bookInventory.TryGetValue(bookId, out int current) || current < amount)
            {
                Debug.LogWarning($"[DataManager] Not enough books: {bookId} (have {current}, need {amount})");
                return false;
            }

            _bookInventory[bookId] = current - amount;
            EventManager.OnInventoryChanged?.Invoke(bookId, _bookInventory[bookId]);
            return true;
        }

        /// <summary>
        /// Snapshot toàn bộ inventory hiện tại (read-only view).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetInventorySnapshot() => _bookInventory;

        /// <summary>
        /// Weighted random chọn loại khách hàng dựa trên spawnWeight trong config.
        /// </summary>
        public CustomerSpawnRule GetRandomCustomerSpawnRule()
        {
            if (_customerSpawnRules == null || _customerSpawnRules.Count == 0)
            {
                Debug.LogError("[DataManager] No customer spawn rules loaded!");
                return null;
            }

            int totalWeight = 0;
            foreach (var rule in _customerSpawnRules)
                totalWeight += rule.spawnWeight;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var rule in _customerSpawnRules)
            {
                cumulative += rule.spawnWeight;
                if (roll < cumulative)
                    return rule;
            }

            // Fallback — không bao giờ xảy ra nếu data hợp lệ
            return _customerSpawnRules[0];
        }

        /// <summary>
        /// Mở kiện hàng: roll ngẫu nhiên từng genre slot, trả về danh sách sách nhận được.
        /// </summary>
        public List<BookData> RollCrateDrop(string crateId)
        {
            var result = new List<BookData>();

            if (!_crates.TryGetValue(crateId, out var crate))
            {
                Debug.LogError($"[DataManager] Crate not found: {crateId}");
                return result;
            }

            foreach (var slot in crate.genreSlots)
            {
                int count = Random.Range(slot.dropMin, slot.dropMax + 1);
                var booksInGenre = GetBooksByGenre(slot.genre);

                if (booksInGenre.Count == 0)
                {
                    Debug.LogWarning($"[DataManager] No books found for genre {slot.genre} in crate {crateId}");
                    continue;
                }

                for (int i = 0; i < count; i++)
                {
                    var randomBook = booksInGenre[Random.Range(0, booksInGenre.Count)];
                    result.Add(randomBook);
                }
            }

            return result;
        }

        /// <summary>
        /// Lấy ngẫu nhiên một dòng dialogue từ category và key.
        /// Ví dụ: GetRandomDialogue("genreHints", "Crime")
        /// </summary>
        public string GetRandomDialogue(string category, string key)
        {
            if (_dialogues == null) return string.Empty;

            Dictionary<string, List<string>> targetDict = category switch
            {
                "greetings"  => _dialogues.greetings,
                "genreHints" => _dialogues.genreHints,
                "responses"  => _dialogues.responses,
                "farewell"   => _dialogues.farewell,
                _            => null
            };

            if (targetDict == null || !targetDict.TryGetValue(key, out var lines) || lines.Count == 0)
                return string.Empty;

            return lines[Random.Range(0, lines.Count)];
        }

        // ==========================================
        // JSON WRAPPER CLASSES (dùng nội bộ)
        // ==========================================

        private class BookDataListWrapper      { public List<BookData>     books;      }
        private class CrateDataListWrapper     { public List<CrateData>    crates;     }
        private class LocationDataListWrapper  { public List<LocationData>  locations;  }
        private class DecorItemDataListWrapper { public List<DecorItemData> decorItems; }
        private class MementoDataListWrapper   { public List<MementoData>  mementos;   }
        private class BookInventoryWrapper     { public Dictionary<string, int> inventory; }

        private class CustomerConfig
        {
            public List<CustomerSpawnRule>   spawnRules;
            public List<SpecialCustomerData> specialCustomers;
        }
    }
}
