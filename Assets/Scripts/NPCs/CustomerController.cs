using UnityEngine;
using System;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Data;

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

        // Data
        public CustomerType CustomerType { get; private set; }
        public int PoolIndex { get; set; }

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
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
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
