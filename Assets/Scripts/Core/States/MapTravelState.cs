namespace WordsOnTheWaves.Core
{
    public class MapTravelState : GameState
    {
        public MapTravelState(GameManager gameManager) : base(gameManager) { }

        public override void Enter()
        {
            // Khi vào state này, GameManager đã phát sự kiện OnGameStateChanged.
            // MapCanvasController sẽ tự động bắt sự kiện và bật giao diện Bản đồ.
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }
}
