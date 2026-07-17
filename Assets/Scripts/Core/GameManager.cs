using UnityEngine;
using UnityEngine.SceneManagement;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameState _currentState;
        
        public string TargetSceneToLoad { get; set; }
        public string CurrentStateName => _currentState != null ? _currentState.GetType().Name : "";

        public MainMenuState MainMenuState { get; private set; }
        public MapTravelState MapTravelState { get; private set; }
        public PreparationState PreparationState { get; private set; }
        public DecorState DecorState { get; private set; }
        // Các state khác sẽ được thêm sau (CargoState, PreparationState, ServiceState...)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeStates();
        }

        private void InitializeStates()
        {
            MainMenuState = new MainMenuState(this);
            MapTravelState = new MapTravelState(this);
            PreparationState = new PreparationState(this);
            DecorState = new DecorState(this);
        }

        private void Start()
        {
            // Lắng nghe sự kiện
            EventManager.OnStartGameClicked += HandleStartGameClicked;
            EventManager.OnMapSelected += HandleMapSelected;
            EventManager.OnDecorButtonClicked += HandleDecorClicked;
            EventManager.OnBackToMapClicked += HandleBackToMapClicked;

            // Xác định state ban đầu dựa trên Scene hiện tại
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                ChangeState(MainMenuState);
            }
            else if (SceneManager.GetActiveScene().name == "playscene")
            {
                ChangeState(MapTravelState);
            }
            else
            {
                // Nếu load vào thẳng bãi biển, sẽ là ServiceState (tạm log để đó)
                Debug.Log("Loaded into a beach scene!");
            }
        }

        private void OnDestroy()
        {
            EventManager.OnStartGameClicked -= HandleStartGameClicked;
            EventManager.OnMapSelected -= HandleMapSelected;
            EventManager.OnDecorButtonClicked -= HandleDecorClicked;
            EventManager.OnBackToMapClicked -= HandleBackToMapClicked;
        }

        private void Update()
        {
            // Zero GC rule: _currentState.Update() không sinh object mới
            _currentState?.Update();
        }

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;

            _currentState?.Exit();
            _currentState = newState;
            
            string stateName = _currentState.GetType().Name;
            Debug.Log($"[GameManager] Changed State to: {stateName}");
            
            // Phát sự kiện toàn cục để UI (Observer) tự động phản hồi
            EventManager.OnGameStateChanged?.Invoke(stateName);

            _currentState?.Enter();
        }

        private void HandleStartGameClicked()
        {
            // Load playscene và chuyển sang MapTravelState
            SceneManager.LoadScene("playscene");
            ChangeState(MapTravelState);
        }

        private void HandleMapSelected(string sceneName)
        {
            // Lưu lại tên scene bãi biển được chọn và đổi trạng thái sang Chuẩn bị
            TargetSceneToLoad = sceneName;
            ChangeState(PreparationState);
        }

        private void HandleDecorClicked()
        {
            ChangeState(DecorState);
        }

        private void HandleBackToMapClicked()
        {
            ChangeState(MapTravelState);
        }
    }
}
