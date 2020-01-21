
namespace Mmm.Platform.IoT.Common.Services.Wrappers
{
    /// <summary>
    /// Mock support
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFactory<out T>
    {
        T Create();
    }
}
