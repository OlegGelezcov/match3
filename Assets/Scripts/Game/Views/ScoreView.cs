namespace Ozh.Game {
    using Ozh.Services;
    using UniRx;
    using UnityEngine.UI;

    public class ScoreView : BaseView {

        public Text scoreText;


        private IGameStateService GameStateService { get; set; }
        private IGridLevelRepository GridLevelRepository { get; set; }


        public override void Setup(IServiceCollection services) {
            base.Setup(services);
            GameStateService = services.Resolve<IGameStateService>();
            GridLevelRepository = services.Resolve<IResourceService>().GetRepository<IGridLevelRepository>();

            var gridLevelData = GridLevelRepository.GetGridLevelData(GameStateService.GridLevel.Value);

            GameStateService.Score.Subscribe(score => {
                scoreText.text = $"{score}/{gridLevelData.winScore}";
            }).AddTo(gameObject);

            GameStateService.GameStateChanged.Subscribe(args => {
                if(args.NewGameState != GameState.Game) {
                    Destroy(gameObject);
                }
            }).AddTo(gameObject);

            scoreText.text = $"{GameStateService.Score.Value}/{gridLevelData.winScore}";
        }
    }

}