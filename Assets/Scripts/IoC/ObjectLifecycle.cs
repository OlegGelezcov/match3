
namespace Ozh.Tools.IoC {
    /// <summary>
    /// How object create - every request new object or singleton object for whole lifecycle of app
    /// </summary>
    public enum ObjectLifecycle
    {
        Singleton,
        Transient
    }
}
