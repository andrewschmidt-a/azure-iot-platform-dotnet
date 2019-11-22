namespace Mmm.Platform.IoT.Common.WebService
{
    public interface IFactory
    {
        T Resolve<T>();
    }
}
