namespace Mmm.Platform.IoT.Common.Services
{
    public interface IFactory
    {
        T Resolve<T>();
    }
}