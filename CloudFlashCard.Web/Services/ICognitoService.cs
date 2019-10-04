using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.CognitoIdentityProvider.Model;
using CloudFlashCard.Web.Models;

namespace CloudFlashCard.Web.Services
{
    public interface ICognitoService
    {
        Task<Result<ConfirmForgotPasswordResponse>> ConfirmForgotPasswordAsync(ConfirmForgotPasswordViewModel model);

        Task<Result<SignUpResponse>> CreateAsync(AwsIdentityUser user);

        Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(string username);

        Task<Result<AwsIdentityUser>> GetUser(string username);

        Task<Result<AwsIdentityUser>> GetUserBySub(string sub);

        Task<Result<AdminInitiateAuthResponse>> LoginAsync(string userName, string password);

        Task<Result<AdminRespondToAuthChallengeResponse>> ResetPasswordAsync(string accessToken, string username, string password);
    }
}
