using System.IO;
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

        /// <summary>Danh sách decorId mà player đang sở hữu.</summary>
        public IReadOnlyList<string> OwnedDecors => _decorInventory;

        /// <summary>Toàn bộ dialogue của game.</summary>
        public DialogueData Dialogues => _dialogues;

        /// <summary>Cấu hình tiền tệ.</summary>
        public CoinConfig CoinConfig => _coinConfig;

        /// <summary>Số lượng tiền hiện tại của người chơi.</summary>
        public int CurrentCoins => _playerProfile != null ? _playerProfile.currentCoins : 0;

        /// <summary>Danh sách quy tắc spawn khách hàng (dùng cho weighted random).</summary>
        public IReadOnlyList<CustomerSpawnRule> CustomerSpawnRules => _customerSpawnRules;

        /// <summary>Danh sách Special Customer với thông tin memento riêng.</summary>
        public IReadOnlyList<SpecialCustomerData> SpecialCustomers => _specialCustomers;

        /// <summary>Truy xuất nhanh các thể loại gần giống.</summary>
        public IReadOnlyDictionary<BookGenre, List<BookGenre>> GenreMatrix => _genreMatrix;

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
        private Dictionary<BookGenre, List<BookGenre>> _genreMatrix = new Dictionary<BookGenre, List<BookGenre>>();
        private PlayerProfile          _playerProfile;

        // Cache danh sách sách theo genre để GetBooksByGenre() không phải duyệt lại mỗi lần
        private readonly Dictionary<BookGenre, List<BookData>> _booksByGenre = new Dictionary<BookGenre, List<BookData>>();

        // Tồn kho sách hiện tại của người chơi (mutable — thay đổi trong gameplay)
        // Key: book id | Value: số lượng đang có
        private readonly Dictionary<string, int> _bookInventory = new Dictionary<string, int>();

        // Danh sách decorId mà player đang sở hữu
        private readonly List<string> _decorInventory = new List<string>();

        // Lượng sách được xếp lên xe ngựa mang đi bãi biển (mutable)
        // Key: book id | Value: số lượng
        private readonly Dictionary<string, int> _wagonCargo = new Dictionary<string, int>();

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

            EventManager.OnPreparationConfirmed += HandlePreparationConfirmed;

            LoadAllData();
        }

        private void OnDestroy()
        {
            EventManager.OnPreparationConfirmed -= HandlePreparationConfirmed;
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
            LoadDecorInventory();
            LoadGenreMatrix();
            LoadPlayerProfile();

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
            string savePath = Path.Combine(Application.persistentDataPath, "book_inv.json");
            string text = null;
            if (File.Exists(savePath))
            {
                text = File.ReadAllText(savePath);
            }
            else
            {
                text = LoadJson("Data/book_inv");
            }

            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<BookInventoryWrapper>(text);
            if (wrapper?.inventory == null) 
            {
                Debug.LogError("[DataManager] Book inventory wrapper or inventory dictionary is null!");
                return;
            }

            foreach (var entry in wrapper.inventory)
                _bookInventory[entry.Key] = entry.Value;
            
            Debug.Log($"[DataManager] Loaded {_bookInventory.Count} book inventory entries.");
        }

        public void SaveBookInventory()
        {
            var wrapper = new BookInventoryWrapper { inventory = new Dictionary<string, int>(_bookInventory) };
            string savePath = Path.Combine(Application.persistentDataPath, "book_inv.json");
            string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            File.WriteAllText(savePath, json);
        }

        private void LoadDecorInventory()
        {
            var text = LoadJson("Data/decor_inv");
            if (text == null) return;

            var wrapper = JsonConvert.DeserializeObject<DecorInventoryWrapper>(text);
            if (wrapper?.ownedDecors == null) return;

            foreach (var decorId in wrapper.ownedDecors)
                _decorInventory.Add(decorId);
        }

        private void LoadGenreMatrix()
        {
            var text = LoadJson("Data/genre_matrix_config");
            if (text == null) return;
            
            var config = JsonConvert.DeserializeObject<GenreMatrixConfig>(text);
            if (config?.matrix == null) return;

            foreach (var item in config.matrix)
            {
                _genreMatrix[item.genre] = item.nearGenres;
            }
        }

        private void LoadPlayerProfile()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    _playerProfile = JsonConvert.DeserializeObject<PlayerProfile>(json);
                    Debug.Log($"[DataManager] Loaded PlayerProfile. Coins: {_playerProfile.currentCoins}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DataManager] Failed to load player profile: {e.Message}");
                }
            }

            // Nếu file không tồn tại hoặc lỗi, khởi tạo mới
            if (_playerProfile == null)
            {
                _playerProfile = new PlayerProfile
                {
                    currentCoins = _coinConfig != null ? _coinConfig.startingCoins : 0
                };
                SavePlayerProfile();
                Debug.Log($"[DataManager] Created new PlayerProfile. Starting Coins: {_playerProfile.currentCoins}");
            }
            
            // Kích hoạt event ban đầu
            EventManager.OnMoneyUpdated?.Invoke(_playerProfile.currentCoins);
        }

        private void SavePlayerProfile()
        {
            if (_playerProfile == null) return;
            string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            string json = JsonConvert.SerializeObject(_playerProfile, Formatting.Indented);
            File.WriteAllText(savePath, json);
        }

        public void AddCoins(int amount)
        {
            if (_playerProfile == null) return;
            _playerProfile.currentCoins += amount;
            SavePlayerProfile();
            EventManager.OnMoneyUpdated?.Invoke(_playerProfile.currentCoins);
        }

        public bool TrySpendCoins(int amount)
        {
            if (_playerProfile == null || _playerProfile.currentCoins < amount)
                return false;

            _playerProfile.currentCoins -= amount;
            SavePlayerProfile();
            EventManager.OnMoneyUpdated?.Invoke(_playerProfile.currentCoins);
            return true;
        }

        public int GetTravelFeeBySceneName(string sceneName)
        {
            foreach (var loc in _locations.Values)
            {
                if (loc.sceneName == sceneName)
                    return loc.travelFee;
            }
            return 0; // Default to 0 if not found
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
        /// Lấy danh sách các thể loại gần giống.
        /// </summary>
        public List<BookGenre> GetNearGenres(BookGenre genre)
        {
            return _genreMatrix.TryGetValue(genre, out var list) ? list : new List<BookGenre>();
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
            SaveBookInventory();
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
            SaveBookInventory();
            EventManager.OnInventoryChanged?.Invoke(bookId, _bookInventory[bookId]);
            return true;
        }

        /// <summary>
        /// Snapshot toàn bộ inventory hiện tại (read-only view).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetInventorySnapshot() => _bookInventory;

        /// <summary>
        /// Thêm một decor item vào inventory. Bỏ qua nếu đã sở hữu.
        /// Phát OnDecorInventoryChanged sau khi cập nhật.
        /// </summary>
        public void AddDecorToInventory(string decorId)
        {
            if (string.IsNullOrEmpty(decorId)) return;

            if (_decorInventory.Contains(decorId))
            {
                Debug.LogWarning($"[DataManager] Decor '{decorId}' đã có trong inventory.");
                return;
            }

            if (!_decorItems.ContainsKey(decorId))
            {
                Debug.LogError($"[DataManager] Decor ID không tồn tại trong config: '{decorId}'");
                return;
            }

            _decorInventory.Add(decorId);
            EventManager.OnDecorInventoryChanged?.Invoke(decorId);
        }

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
                {
                    Debug.Log($"[DataManager] Random Rule Roll: {roll}/{totalWeight} => Type: {rule.customerType} (Weight: {rule.spawnWeight})");
                    return rule;
                }
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
        // WAGON CARGO SYSTEM
        // ==========================================

        private void HandlePreparationConfirmed(Dictionary<string, int> cargoData)
        {
            _wagonCargo.Clear();
            foreach (var entry in cargoData)
            {
                _wagonCargo[entry.Key] = entry.Value;
            }
            Debug.Log($"[DataManager] Bắt đầu chuyến đi với {_wagonCargo.Count} loại sách trên xe.");
        }

        /// <summary>
        /// Snapshot lượng sách đang có trên xe (ServiceState dùng).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetWagonCargo() => _wagonCargo;

        /// <summary>
        /// Bán một cuốn sách từ xe ngựa.
        /// </summary>
        public bool SellBookFromWagon(string bookId)
        {
            if (!_wagonCargo.TryGetValue(bookId, out int count) || count <= 0)
                return false;
            
            _wagonCargo[bookId] = count - 1;
            // Gọi sự kiện OnMoneyUpdated hoặc tương tự tùy logic
            return true;
        }

        /// <summary>
        /// Trả toàn bộ sách dư trên xe về kho khi kết thúc ngày.
        /// </summary>
        public void ReturnCargoToInventory()
        {
            int returnedCount = 0;
            foreach (var entry in _wagonCargo)
            {
                if (entry.Value > 0)
                {
                    AddBooks(entry.Key, entry.Value);
                    returnedCount += entry.Value;
                }
            }
            _wagonCargo.Clear();
            Debug.Log($"[DataManager] Kết thúc ngày. Đã trả {returnedCount} cuốn sách dư về kho.");
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
        private class DecorInventoryWrapper    { public List<string> ownedDecors; }

        private class CustomerConfig
        {
            public List<CustomerSpawnRule>   spawnRules;
            public List<SpecialCustomerData> specialCustomers;
        }
    }
}
