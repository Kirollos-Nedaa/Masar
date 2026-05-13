using Google.Apis.Auth;

namespace Masar.Core.IService
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleJsonWebSignature.Payload?> ValidateAsync(string idToken);
    }
}
