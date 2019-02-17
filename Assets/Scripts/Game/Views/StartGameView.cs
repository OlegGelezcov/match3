namespace Ozh.Game {
    using Ozh.Services;
    using UnityEngine.UI;

    public class StartGameView : BaseView {

        public Button startGameButton;

        private IGameStateService GameStateService { get; set; }

        public override void Setup(IServiceCollection services) {
            base.Setup(services);
            GameStateService = services.Resolve<IGameStateService>();

            startGameButton.SetListener(() => {
                GameStateService.StartGame();
                Destroy(gameObject);
            });
        }
    }

}