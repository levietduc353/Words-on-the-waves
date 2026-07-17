namespace WordsOnTheWaves.Core
{
    public abstract class GameState
    {
        protected GameManager _gameManager;

        public GameState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}
