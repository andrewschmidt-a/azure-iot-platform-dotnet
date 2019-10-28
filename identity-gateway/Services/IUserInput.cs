namespace IdentityGateway.Services
{
    public interface IUserInput<TModel>
    {
        string UserId { get; set; }
    }
}