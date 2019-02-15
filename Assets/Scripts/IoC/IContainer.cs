namespace Ozh.Tools.IoC {
    public interface IContainer
    {
        IObjectBuilder AddTransient<ITypeToResolve, TConcrete>();

        IObjectBuilder AddTransient<TConcrete>();

        IObjectBuilder AddSingleton<ITypeToResolve, TConcrete>();

        IObjectBuilder AddSingleton<TConcrete>();

        ITypeToResolve Resolve<ITypeToResolve>();
        ITypeToResolve Resolve<ITypeToResolve>(string id);

        void Build();

    }
}
