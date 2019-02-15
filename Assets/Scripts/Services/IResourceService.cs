namespace Ozh.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IResourceService : IGameService  {
        GameObject GetPrefab(string key);

        T GetRepository<T>() where T : class, IRepository;
    }


}
