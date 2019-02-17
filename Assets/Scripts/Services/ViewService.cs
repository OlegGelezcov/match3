namespace Ozh.Services {
    using Ozh.Game;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;
    using System.Collections;

    public class ViewService : MonoBehaviour, IViewService {

        public Canvas canvas;

        private IPrefabRepository<string> PrefabRepository { get; set; }
        private IGameStateService GameStateService { get; set; }
        private IServiceCollection ServiceCollection { get; set; }

        public void Setup(IServiceCollection services ) {
            ServiceCollection = services;
            PrefabRepository = services.Resolve<IResourceService>().GetRepository<IPrefabRepository<string>>();
            GameStateService = services.Resolve<IGameStateService>();
        }

        private void Start() {

            GameStateService.GameStateChanged.Subscribe(args => {
                if (args.NewGameState == GameState.StartGameMenu) {
                    ShowView(ViewType.StartGameView, 1);
                } else if (args.NewGameState == GameState.EndGameMenu) {
                    ShowView(ViewType.EndGameView, 2);
                } else if(args.NewGameState == GameState.Game ) {
                    ShowView(ViewType.ScoreView);
                }
            }).AddTo(gameObject);

            if (GameStateService.State == GameState.StartGameMenu) {
                ShowView(ViewType.StartGameView, 1);
            }
        }

        private void ShowView(ViewType type, float delay ) {
            StartCoroutine(ShowViewDelayed(type, delay));
        }

        private IEnumerator ShowViewDelayed(ViewType type, float delay) {
            yield return new WaitForSeconds(delay);
            ShowView(type);
        }

        private readonly Dictionary<ViewType, string> viewTypeResourceNameMap
            = new Dictionary<ViewType, string> {
                [ViewType.StartGameView] = "startgameview",
                [ViewType.EndGameView] = "endgameview",
                [ViewType.ScoreView] = "scoreview"
            };

        public void ShowView(ViewType viewType) {
            if(viewTypeResourceNameMap.ContainsKey(viewType)) {             
                string prefabName = viewTypeResourceNameMap[viewType];
                GameObject prefab = PrefabRepository.GetPrefab(prefabName);
                var view = prefab.MakeUIInstance(canvas.transform).GetComponent<BaseView>();
                view.Setup(ServiceCollection);
            }
        }
    }

    public interface IViewService : IGameService {
        void ShowView(ViewType viewType);
    }

    public enum ViewType {
        StartGameView,
        EndGameView,
        ScoreView
    }
}