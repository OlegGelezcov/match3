namespace Ozh.Game {
    using Ozh.Services;
    using UnityEngine.UI;

    public class EndGameView : BaseView {

        public Button tryAgainButton;

        public override void Setup(IServiceCollection services) {
            base.Setup(services);
            IGameStateService gameStateService = services.Resolve<IGameStateService>();
            tryAgainButton.SetListener(() => {
                gameStateService.EndGame();
                Destroy(gameObject);
            });
        }
    }
}
