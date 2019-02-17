namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IServiceCollection  {

        T Register<T, U>(U instance) where U : class, T where T : IGameService;
        T Resolve<T>() where T : IGameService;
    }

}