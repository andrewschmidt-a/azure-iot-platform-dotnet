namespace Mmm.Platform.IoT.Common.Services.Wrappers
{
    public interface IFactory<out T>
    {
        T Create();
    }
}