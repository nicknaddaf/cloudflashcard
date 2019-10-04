using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CloudFlashCard.Web.Models;

namespace CloudFlashCard.Web.Services
{
    public class CognitoService : ICognitoService
    {
        private readonly IAmazonCognitoIdentityProvider _idpProvider;
        private readonly AppSettings appSettings = new AppSettings();

        public CognitoService(IOptions<AppSettings> settings,
            IAmazonCognitoIdentityProvider idpProvider)
        {
            appSettings = settings.Value;

            AWSConfigs.AWSRegion = appSettings.AwsRegion;

            _idpProvider = idpProvider;
        }

        public async Task<Result<AwsIdentityUser>> GetUser(string username)
        {
            try
            {
                var user = new AwsIdentityUser();

                var userReq = new AdminGetUserRequest
                {
                    Username = username,
                    UserPoolId = appSettings.PoolId
                };

                AdminGetUserResponse userResp = await _idpProvider.AdminGetUserAsync(userReq);

                if (userResp.UserStatus != UserStatusType.CONFIRMED)
                {
                    Result.Fail<AwsIdentityUser>(userResp.UserStatus.Value);
                }

                user.Status = userResp.UserStatus.Value;
                user.Username = username;

                foreach (var att in userResp.UserAttributes)
                {
                    switch (att.Name.ToLower())
                    {
                        case "sub":
                            user.Sub = att.Value;
                            break;
                        case "email":
                            user.Email = att.Value;
                            break;
                        case "address":
                            user.Address = att.Value;
                            break;
                        case "family_name":
                            user.Family_Name = att.Value;
                            break;
                        case "given_name":
                            user.Given_Name = att.Value;
                            break;
                        case "locale":
                            user.Locale = att.Value;
                            break;
                        case "profile":
                            user.Profile = att.Value;
                            break;
                    }
                }

                user.Id = user.Sub;

                return Result.Ok(user);
            }
            catch (Exception e)
            {
                return Result.Fail<AwsIdentityUser>(e.Message);
            }
        }

        public async Task<Result<AwsIdentityUser>> GetUserBySub(string sub)
        {
            try
            {
                var listUsersReq = new ListUsersRequest
                {
                    Filter = "sub=\"" + sub + "\"",
                    UserPoolId = appSettings.PoolId
                };

                ListUsersResponse listUsersResp = await _idpProvider.ListUsersAsync(listUsersReq);

                if (listUsersResp.Users.Count != 1)
                    return Result.Fail<AwsIdentityUser>("No user found");

                var user = await this.GetUser(listUsersResp.Users[0].Username);

                if (user.IsFailure)
                    return Result.Fail<AwsIdentityUser>(user.Error);
                else
                    return Result.Ok(user.Value);
            }
            catch (Exception e)
            {
                return Result.Fail<AwsIdentityUser>(e.Message);
            }
        }

        public async Task<Result<SignUpResponse>> CreateAsync(AwsIdentityUser user)
        {
            try
            {
                var signUpRequest = new SignUpRequest
                {
                    ClientId = appSettings.ClientId,
                    Password = user.Password,
                    Username = user.Email,
                    
                };

                var emailAttribute = new AttributeType
                {
                    Name = "email",
                    Value = user.Email
                };
                signUpRequest.UserAttributes.Add(emailAttribute);

                return Result.Ok(await _idpProvider.SignUpAsync(signUpRequest));
            }
            catch (Exception e)
            {
                return Result.Fail<SignUpResponse>(e.Message);
            }

        }

        public async Task<Result<AdminInitiateAuthResponse>> LoginAsync(string userName, string password)
        {
            try
            {
                var authReq = new AdminInitiateAuthRequest
                {
                    UserPoolId = appSettings.PoolId,
                    ClientId = appSettings.ClientId,
                    AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH
                };
                authReq.AuthParameters.Add("USERNAME", userName);
                authReq.AuthParameters.Add("PASSWORD", password);

                AdminInitiateAuthResponse authResp = await _idpProvider.AdminInitiateAuthAsync(authReq);

                return Result.Ok(authResp);
            }
            catch (Exception e)
            {
                return Result.Fail<AdminInitiateAuthResponse>(e.Message);
            }
        }

        public async Task<Result<AdminRespondToAuthChallengeResponse>> ResetPasswordAsync(string accessToken, string username, string password)
        {
            try
            {
                var challengeResponses = new Dictionary<string, string>();
                challengeResponses.Add("USERNAME", username);
                challengeResponses.Add("NEW_PASSWORD", password);

                var authChallenge = new AdminRespondToAuthChallengeRequest
                {
                    ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                    ChallengeResponses = challengeResponses,
                    Session = accessToken,
                    ClientId = appSettings.ClientId,
                    UserPoolId = appSettings.PoolId
                };

                var authChallengeResp = await _idpProvider.AdminRespondToAuthChallengeAsync(authChallenge);

                return Result.Ok(authChallengeResp);
            }
            catch (Exception e)
            {
                return Result.Fail<AdminRespondToAuthChallengeResponse>(e.Message);
            }
        }

        public async Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(string username)
        {
            try
            {
                var forgotPassReq = new ForgotPasswordRequest
                {
                    ClientId = appSettings.ClientId,
                    Username = username
                };
                var forgotPassResp = await _idpProvider.ForgotPasswordAsync(forgotPassReq);
                return Result.Ok(forgotPassResp);
            }
            catch (Exception e)
            {
                return Result.Fail<ForgotPasswordResponse>(e.Message);
            }
        }

        public async Task<Result<ConfirmForgotPasswordResponse>> ConfirmForgotPasswordAsync(ConfirmForgotPasswordViewModel model)
        {
            try
            {
                var forgotPassReq = new ConfirmForgotPasswordRequest
                {
                    ClientId = appSettings.ClientId,
                    Username = model.Email,
                    Password = model.Password,
                    ConfirmationCode = model.Code
                };
                var forgotPassResp = await _idpProvider.ConfirmForgotPasswordAsync(forgotPassReq);
                return Result.Ok(forgotPassResp);
            }
            catch (Exception e)
            {
                return Result.Fail<ConfirmForgotPasswordResponse>(e.Message);
            }
        }
    }
}
