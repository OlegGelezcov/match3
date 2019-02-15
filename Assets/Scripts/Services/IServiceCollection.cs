namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IServiceCollection  {

        T Resolve<T>();
    }

}