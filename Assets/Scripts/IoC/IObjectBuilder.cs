
using System;

namespace Ozh.Tools.IoC {
    public interface IObjectBuilder {
        IObjectBuilder AsLazy();
        IObjectBuilder AsNonLazy();
        IObjectBuilder WithFabric<TConcrete>(Func<TConcrete> fabric);
        IObjectBuilder WithId(string id);
    }
}

