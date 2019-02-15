namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IRepository  {

        void Load(object obj = null);

        bool IsLoaded { get; }
    }

}