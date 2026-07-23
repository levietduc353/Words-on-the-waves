using UnityEngine;
using System;
using System.Collections.Generic;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.NPCs
{
    public enum CustomerState
    {
        Idle,
        WalkingIn,
        Waiting,
        WalkingToC,
        WalkingOut
    }

    /// <summary>
    /// Điều khiển vòng đời của 1 NPC (Khách hàng) bằng Mini-FSM nội bộ.
    /// Không dùng boolean flags. Dùng Vector3.MoveTowards cho hiệu năng Zero GC.
    /// </summary>
    public class CustomerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _waitDuration = 5f;
        [SerializeField] private Animator _animator;

        private CustomerState _currentState = CustomerState.Idle;
        private Vector3 _targetPos;
        
        private Vector3 _posStop;
        private Vector3 _posB;
        private Vector3 _posC;
        private bool _hasPosC;
        
        private float _waitTimer;
        private Action<CustomerController> _onWalkOutComplete;

        [Header("Debug Inspector")]
        [SerializeField] private CustomerType _debugCustomerType;

        // Data
        public CustomerType CustomerType { get; private set; }
        public int PoolIndex { get; set; }
        public BookGenre RequestedGenre { get; private set; }
        public string SpecialCustomerId { get; private set; }

        private bool _isWaitingForAdvice = false;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// Khởi tạo và bắt đầu chu trình của NPC.
        /// Gọi bởi CustomerSpawner khi lấy từ Pool.
        /// </summary>
        public void InitAndStart(CustomerType type, Vector3 spawnPos, Vector3 posStop, Vector3 posB, bool hasPosC, Vector3 posC, Action<CustomerController> onComplete)
        {
            CustomerType = type;
            _debugCustomerType = type; // Hiển thị trên Inspector
            
            _isWaitingForAdvice = false;
            
            if (type == CustomerType.NeedAdvice)
            {
                Array genres = Enum.GetValues(typeof(BookGenre));
                RequestedGenre = (BookGenre)genres.GetValue(UnityEngine.Random.Range(0, genres.Length));
            }
            else if (type == CustomerType.Special)
            {
                var specials = DataManager.Instance.SpecialCustomers;
                if (specials != null && specials.Count > 0)
                {
                    var special = specials[UnityEngine.Random.Range(0, specials.Count)];
                    SpecialCustomerId = special.id;
                    RequestedGenre = special.preferredGenre;
                }
            }

            transform.position = spawnPos;
            _posStop = posStop;
            _posB = posB;
            _hasPosC = hasPosC;
            _posC = posC;
            _onWalkOutComplete = onComplete;
            
            // Xoay mặt về hướng điểm đến
            LookAtTarget(_posStop);

            ChangeState(CustomerState.WalkingIn);
        }

        private void Update()
        {
            switch (_currentState)
            {
                case CustomerState.WalkingIn:
                    HandleWalkingIn();
                    break;
                case CustomerState.Waiting:
                    HandleWaiting();
                    break;
                case CustomerState.WalkingToC:
                    HandleWalkingToC();
                    break;
                case CustomerState.WalkingOut:
                    HandleWalkingOut();
                    break;
            }
        }

        private void ChangeState(CustomerState newState)
        {
            _currentState = newState;
            
            switch (_currentState)
            {
                case CustomerState.WalkingIn:
                    _targetPos = _posStop;
                    SetAnimationWalk(true);
                    break;
                    
                case CustomerState.Waiting:
                    SetAnimationWalk(false);
                    _waitTimer = _waitDuration;
                    // Phát event cho UI/Core biết khách đã tới quầy
                    EventManager.OnCustomerArrived?.Invoke(this.gameObject);
                    break;
                    
                case CustomerState.WalkingToC:
                    _targetPos = _posC;
                    LookAtTarget(_posC);
                    SetAnimationWalk(true);
                    break;

                case CustomerState.WalkingOut:
                    _targetPos = _posB;
                    LookAtTarget(_posB);
                    SetAnimationWalk(true);
                    // Có thể phát event khách rời đi ngay lúc quay lưng, hoặc lúc despawn
                    break;
            }
        }

        private void HandleWalkingIn()
        {
            MoveTowardsTarget();
            if (Vector3.Distance(transform.position, _targetPos) < 0.05f)
            {
                ChangeState(CustomerState.Waiting);
            }
        }

        private void HandleWaiting()
        {
            if (_isWaitingForAdvice) return;

            // Nếu vừa mới đến và là khách cần tư vấn
            if (_waitTimer == _waitDuration && (CustomerType == CustomerType.NeedAdvice || CustomerType == CustomerType.Special))
            {
                _isWaitingForAdvice = true;
                EventManager.OnCustomerNeedsAdvice?.Invoke(this, RequestedGenre, CustomerType);
                return;
            }

            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                // Tự động mua sách đối với khách Normal
                if (CustomerType == CustomerType.Normal)
                {
                    AutoBuyRandomBookFromWagon();
                }

                EventManager.OnCustomerLeftCounter?.Invoke(this.gameObject);
                
                if (_hasPosC)
                {
                    ChangeState(CustomerState.WalkingToC);
                }
                else
                {
                    ChangeState(CustomerState.WalkingOut);
                }
            }
        }

        private void AutoBuyRandomBookFromWagon()
        {
            var wagon = DataManager.Instance.GetWagonCargo();
            List<string> availableBooks = new List<string>();
            foreach (var kvp in wagon)
            {
                if (kvp.Value > 0) availableBooks.Add(kvp.Key);
            }

            if (availableBooks.Count > 0)
            {
                // Chọn ngẫu nhiên 1 cuốn sách
                string selectedBookId = availableBooks[UnityEngine.Random.Range(0, availableBooks.Count)];
                
                // Thực hiện mua
                DataManager.Instance.SellBookFromWagon(selectedBookId);
                
                if (DataManager.Instance.Books.TryGetValue(selectedBookId, out var bookData))
                {
                    float multiplier = 1f;
                    var rules = DataManager.Instance.CustomerSpawnRules;
                    foreach (var rule in rules)
                    {
                        if (rule.customerType == CustomerType.Normal)
                        {
                            multiplier = rule.bonusMultiplier;
                            break;
                        }
                    }
                    
                    int totalCoins = Mathf.RoundToInt(bookData.sellPrice * multiplier);
                    DataManager.Instance.AddCoins(totalCoins);
                    
                    Debug.Log($"[CustomerController] Normal customer auto-bought '{selectedBookId}' for {totalCoins} coins.");
                }
            }
            else
            {
                Debug.Log("[CustomerController] Normal customer wanted to buy but wagon is empty.");
            }
        }

        public void OnAdviceReceived(bool isAccepted)
        {
            _isWaitingForAdvice = false;
            _waitTimer = 0f; // Bắt buộc rời đi ở frame sau
        }

        private void HandleWalkingToC()
        {
            MoveTowardsTarget();
            if (Vector3.Distance(transform.position, _targetPos) < 0.05f)
            {
                ChangeState(CustomerState.WalkingOut);
            }
        }

        private void HandleWalkingOut()
        {
            MoveTowardsTarget();
            if (Vector3.Distance(transform.position, _targetPos) < 0.05f)
            {
                ChangeState(CustomerState.Idle);
                SetAnimationWalk(false);
                
                EventManager.OnCustomerLeft?.Invoke(this.gameObject);
                
                // Trả về pool
                _onWalkOutComplete?.Invoke(this);
            }
        }

        private void MoveTowardsTarget()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _walkSpeed * Time.deltaTime);
        }

        private void LookAtTarget(Vector3 target)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0; // Giữ nguyên độ cao, chỉ xoay trục Y
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        private void SetAnimationWalk(bool isWalking)
        {
            if (_animator != null)
            {
                _animator.SetBool("IsWalking", isWalking);
            }
        }
    }
}
