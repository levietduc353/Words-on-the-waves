using UnityEngine;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.NPCs
{
    /// <summary>
    /// Quản lý việc sinh (spawn) khách hàng tại bãi biển.
    /// Tái sử dụng NPC thông qua ObjectPool thay vì Instantiate liên tục (Zero GC).
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Danh sách Prefab của khách hàng có gắn script CustomerController (Hỗ trợ 4 slots hoặc hơn)")]
        [SerializeField] private CustomerController[] _customerPrefabs;

        [Header("Waypoints")]
        [SerializeField] private Transform _spawnPointA;
        [SerializeField] private Transform _stopPoint;
        [SerializeField] private Transform _intermediatePointC;
        [SerializeField] private Transform _despawnPointB;

        [Header("Spawn Settings")]
        [SerializeField] private float _minSpawnDelay = 3f;
        [SerializeField] private float _maxSpawnDelay = 8f;
        
        [Tooltip("Có cho phép tự động sinh NPC hay không (Kích hoạt khi vào ServiceState)")]
        public bool isSpawningActive = true;

        private ObjectPool<CustomerController>[] _customerPools;
        private float _spawnTimer;
        private bool _isPausedByAdviceCustomer = false;

        private void OnEnable()
        {
            EventManager.OnCustomerLeftCounter += HandleCustomerLeftCounter;
        }

        private void OnDisable()
        {
            EventManager.OnCustomerLeftCounter -= HandleCustomerLeftCounter;
        }

        private void HandleCustomerLeftCounter(GameObject customerObj)
        {
            var customer = customerObj.GetComponent<CustomerController>();
            if (customer != null && (customer.CustomerType == CustomerType.NeedAdvice || customer.CustomerType == CustomerType.Special))
            {
                _isPausedByAdviceCustomer = false;
                ResetSpawnTimer();
            }
        }

        private void Awake()
        {
            if (_customerPrefabs == null || _customerPrefabs.Length == 0)
            {
                Debug.LogError("[CustomerSpawner] Cần gắn ít nhất 1 Prefab vào mảng _customerPrefabs!");
                return;
            }

            // Khởi tạo mảng Pool tương ứng với số lượng Prefab
            _customerPools = new ObjectPool<CustomerController>[_customerPrefabs.Length];
            for (int i = 0; i < _customerPrefabs.Length; i++)
            {
                _customerPools[i] = new ObjectPool<CustomerController>(_customerPrefabs[i], 5, 20, transform);
            }
            
            ResetSpawnTimer();
        }

        private void Update()
        {
            if (!isSpawningActive || _isPausedByAdviceCustomer) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnCustomer();
                ResetSpawnTimer();
            }
        }

        private void SpawnCustomer()
        {
            if (_spawnPointA == null || _stopPoint == null || _despawnPointB == null)
            {
                Debug.LogWarning("[CustomerSpawner] Thiếu Waypoints! Hãy gán Transform trong Inspector.");
                return;
            }
            if (_customerPrefabs == null || _customerPrefabs.Length == 0) return;

            // Chọn ngẫu nhiên 1 prefab (slot)
            int randomPrefabIndex = Random.Range(0, _customerPrefabs.Length);
            
            // Lấy NPC từ Pool tương ứng
            CustomerController customer = _customerPools[randomPrefabIndex].Get();
            customer.PoolIndex = randomPrefabIndex;

            // Đọc dữ liệu CustomerType từ DataManager (Data-driven)
            CustomerType spawnType = CustomerType.Normal;
            if (DataManager.Instance != null)
            {
                var rule = DataManager.Instance.GetRandomCustomerSpawnRule();
                if (rule != null)
                {
                    spawnType = rule.customerType;
                }
            }

            // Kiểm tra posC có tồn tại không
            bool hasPosC = _intermediatePointC != null;
            Vector3 posC = hasPosC ? _intermediatePointC.position : Vector3.zero;

            if (spawnType == CustomerType.NeedAdvice || spawnType == CustomerType.Special)
            {
                _isPausedByAdviceCustomer = true;
                Debug.Log($"[CustomerSpawner] Đã spawn khách hàng {spawnType}. Tạm dừng đếm ngược spawn mới!");
            }

            // Khởi động vòng đời FSM cho NPC
            customer.InitAndStart(
                spawnType,
                _spawnPointA.position,
                _stopPoint.position,
                _despawnPointB.position,
                hasPosC,
                posC,
                OnCustomerWalkOutComplete // Callback khi NPC đi tới posB
            );
        }

        private void OnCustomerWalkOutComplete(CustomerController customer)
        {
            // Trả NPC về đúng Pool của nó để tái sử dụng
            if (customer.PoolIndex >= 0 && customer.PoolIndex < _customerPools.Length)
            {
                _customerPools[customer.PoolIndex].Release(customer);
            }
        }

        private void ResetSpawnTimer()
        {
            _spawnTimer = Random.Range(_minSpawnDelay, _maxSpawnDelay);
        }

        // ==========================================
        // PUBLIC API
        // ==========================================

        public void StartSpawning()
        {
            isSpawningActive = true;
            ResetSpawnTimer();
        }

        public void StopSpawning()
        {
            isSpawningActive = false;
        }
    }
}
