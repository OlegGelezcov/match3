namespace Ozh.Game {
    using Ozh.Services;
    using UnityEngine;

    public class BaseView : MonoBehaviour {

        protected IServiceCollection Services { get; private set; }

        public virtual void Setup(IServiceCollection services) {
            Services = services;
        }
    }
}
