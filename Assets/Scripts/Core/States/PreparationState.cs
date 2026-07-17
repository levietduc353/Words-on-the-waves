namespace WordsOnTheWaves.Core
{
    public class PreparationState : GameState
    {
        public PreparationState(GameManager gameManager) : base(gameManager) { }

        public override void Enter()
        {
            // FSM chuyển vào trạng thái PreparationState.
            // PrepareCanvasController sẽ bắt sự kiện OnGameStateChanged để hiện UI.
            // CameraController sẽ bắt sự kiện để di chuyển góc nhìn.
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }
}
