namespace Ozh.Services {
    using Ozh.Game;
    using UniRx;
    using UnityEngine;

    public class GameStateService : MonoBehaviour, IGameStateService {


        private IGridService GridService { get; set; }
        private IGridLevelRepository GridLevelRepository { get; set; }

        public GameState State { get; private set; } = GameState.None;
        public Subject<GameStateArgs> GameStateChanged { get; } = new Subject<GameStateArgs>();
        public ReactiveProperty<int> Score { get;  } = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> GridLevel { get; } = new ReactiveProperty<int>(1);

        public void Setup(IServiceCollection services) {
            GridService = services.Resolve<IGridService>();
            GridLevelRepository = services.Resolve<IResourceService>().GetRepository<IGridLevelRepository>();
        }

        private void Start() {
            SetGameState(GameState.StartGameMenu);
            Score.Subscribe(value => {
                if (State == GameState.Game) {
                    var gridLevelData = GridLevelRepository.GetGridLevelData(GridLevel.Value);
                    if (value >= gridLevelData.winScore) {
                        SetGameState(GameState.EndGameMenu);
                    }
                }
            }).AddTo(gameObject);
            GameStateChanged.Subscribe(args => {
                if (State == GameState.StartGameMenu) {
                    Score.Value = 0;
                }
            }).AddTo(gameObject);
        }

        public void StartGame() {
            GridService.MakeGrid(GridLevel.Value);
            SetGameState(GameState.Game);
        }

        public void EndGame() {
            if(State == GameState.EndGameMenu) {
                GridService.ClearGrid();
                SetGameState(GameState.StartGameMenu);
            }
        }

        private void SetGameState(GameState newState) {
            var oldState = State;
            State = newState;
            if(State != oldState) {
                GameStateChanged.OnNext(new GameStateArgs { OldGameState = oldState, NewGameState = State });
            }
        }
    }

    public interface IGameStateService : IGameService {
        GameState State { get; }
        void StartGame();
        void EndGame();
        Subject<GameStateArgs> GameStateChanged { get; }
        ReactiveProperty<int> Score { get;  }
        ReactiveProperty<int> GridLevel { get; }
    }

    public enum GameState {
        None,
        StartGameMenu,
        Game,
        EndGameMenu
    }

    public class GameStateArgs {
        public GameState OldGameState { get; set; }
        public GameState NewGameState { get; set; }
    }
}
