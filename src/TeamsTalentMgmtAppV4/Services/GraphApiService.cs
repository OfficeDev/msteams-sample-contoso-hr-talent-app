using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Data;

namespace TeamsTalentMgmtAppV4.Services
{
    public class GraphApiService : IGraphApiService
    {
        private readonly AppSettings _appSettings;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<GraphApiService> _logger;
        private readonly IConfiguration _configuration;

        public GraphApiService(
            ILogger<GraphApiService> logger,
            IOptions<AppSettings> appSettings,
            IConfiguration configuration,
            DatabaseContext databaseContext)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _databaseContext = databaseContext;
        }

        public async Task<string> GetProactiveChatIdForUser(string tenantId, string upn, CancellationToken cancellationToken)
        {
            var token = await GetTokenForApp(tenantId);

            var graphClient = GetGraphServiceClient(token);

            var installedApps = await graphClient.Users[upn].Teamwork.InstalledApps
                .Request()
                .Filter($"teamsApp/externalId eq '{_configuration["TeamsAppId"]}'")
                .Expand("teamsApp")
                .GetAsync(cancellationToken);

            var app = installedApps.FirstOrDefault();
            if (app == null)
            {
                return null;
            }

            var chat = await graphClient.Users[upn].Teamwork.InstalledApps[app.Id].Chat
                .Request()
                .GetAsync(cancellationToken);

            return chat.Id;
        }

        public async Task<string> GetDomainForUser(string token, CancellationToken cancellationToken)
        {
            var delgatedAccessToken = await GetOnBehalfOfToken(token);
            var graphClient = GetGraphServiceClient(delgatedAccessToken);

            var requester = await graphClient.Me.Request().GetAsync(cancellationToken);
            var domain = new MailAddress(requester.UserPrincipalName).Host;

            return domain;
        }

        public async Task<bool> InstallBotForUser(string tenantId, string alias, CancellationToken cancellationToken)
        {
            var token = await GetTokenForApp(tenantId);

            var graphClient = GetGraphServiceClient(token);

            //var requester = await graphClient.Me.Request().GetAsync(cancellationToken);
            //var domain = new MailAddress(requester.UserPrincipalName).Host;
            //var users = await graphClient.Users.Request().Filter($"userPrincipalName eq '{alias}@{domain}'").GetAsync(cancellationToken);
            //if (users == null || users.Count != 1)
            //{
            //    return false;
            //}

            var teamsApps = await graphClient
                .AppCatalogs
                .TeamsApps
                .Request()
                .Filter($"distributionMethod eq 'organization' and externalId eq '{_appSettings.TeamsAppId}'")
                .GetAsync(cancellationToken);

            var teamApp = teamsApps.FirstOrDefault();
            var success = false;
            if (!string.IsNullOrEmpty(teamApp?.Id))
            {
                try
                {
                    var installBotRequest = new BaseRequest($"https://graph.microsoft.com/v1.0/users/{alias}/teamwork/installedApps", graphClient)
                    {
                        Method = HttpMethods.POST,
                        ContentType = MediaTypeNames.Application.Json
                    };

                    await installBotRequest.SendAsync(
                        new TeamsAppInstallation
                    {
                        AdditionalData = new Dictionary<string, object>
                            {
                                { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamApp.Id}" }
                            }
                    }, cancellationToken);

                    // get chat will force Welcome message where we will save information about user.
                    // https://github.com/microsoftgraph/microsoft-graph-docs/issues/5547
                    await new BaseRequest($"https://graph.microsoft.com/v1.0/users/{alias}/chats?$filter=installedApps/any(a:a/teamsApp/id eq '{teamApp.Id}')", graphClient)
                    {
                        Method = HttpMethods.GET
                    }.SendAsync(null, cancellationToken);

                    success = true;
                }
                catch (Exception ex)
                {
                    // We don't want to show any exception for user in this case.
                    success = false;
                    _logger.LogError(ex, ex.Message);
                }
            }

            return success;
        }

        public async Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken)
        {
            var graphClient = GetGraphServiceClient(token);

            // If you have a user's UPN, you can add it directly to a group, but then there will be a
            // significant delay before Microsoft Teams reflects the change. Instead, we find the user
            // object's id, and add the ID to the group through the Graph beta endpoint, which is
            // recognized by Microsoft Teams much more quickly. See
            // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/teams_api_overview
            // for more about delays with adding members.
            var requester = await graphClient.Me.Request().GetAsync(cancellationToken);

            var createdGroup = await graphClient.Groups.Request().AddAsync(
                new Group
                {
                    DisplayName = $"Position {position.PositionExternalId}",
                    MailNickname = position.PositionExternalId,
                    Description = $"Everything about position {position.PositionExternalId}",
                    Visibility = "Private",
                    GroupTypes = new[] { "Unified" }, // Office 365 (aka unified group)
                    MailEnabled = true, // true if creating an Office 365 Group
                    SecurityEnabled = false, // false if creating an Office 365 group,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "members@odata.bind", await GetTeamMemberIds(graphClient, position, requester, cancellationToken) },
                        { "owners@odata.bind", await GetTeamOwnerIds(graphClient, position, requester, cancellationToken) }
                    }
                },
                cancellationToken);

            var team = await graphClient.Groups[createdGroup.Id].Team.Request().PutAsync(
                new Team
                {
                    GuestSettings = new TeamGuestSettings
                    {
                        AllowCreateUpdateChannels = false,
                        AllowDeleteChannels = false
                    },
                    MemberSettings = new TeamMemberSettings
                    {
                        AllowCreateUpdateChannels = true
                    },
                    MessagingSettings = new TeamMessagingSettings
                    {
                        AllowUserEditMessages = true,
                        AllowUserDeleteMessages = true
                    },
                    FunSettings = new TeamFunSettings
                    {
                        AllowGiphy = true,
                        GiphyContentRating = GiphyRatingType.Strict
                    }
                }, cancellationToken);

            var channel = await graphClient.Teams[team.Id].Channels.Request().AddAsync(
                new Channel
                {
                    DisplayName = "Candidates",
                    Description = "Discussion about interview, feedback, etc."
                }, cancellationToken);

            var teamsApps = await graphClient.AppCatalogs.TeamsApps.Request().Filter($"distributionMethod eq 'organization' and externalId eq '{_appSettings.TeamsAppId}'").GetAsync(cancellationToken);
            var teamApp = teamsApps.FirstOrDefault();
            if (!string.IsNullOrEmpty(teamApp?.Id))
            {
                var appId = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamApp.Id}";
                await graphClient.Teams[team.Id].InstalledApps.Request().AddAsync(
                    new TeamsAppInstallation
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "teamsApp@odata.bind", appId }
                    }
                }, cancellationToken);

                var contentUrl = $"{_appSettings.BaseUrl}/StaticViews/TeamTab.html?positionId={position.PositionId}";
                await graphClient.Teams[team.Id].Channels[channel.Id].Tabs.Request().AddAsync(
                    new TeamsTab
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "teamsApp@odata.bind", appId }
                    },
                    DisplayName = position.PositionExternalId,
                    Configuration = new TeamsTabConfiguration
                    {
                        EntityId = position.PositionId.ToString(),
                        ContentUrl = contentUrl,
                        WebsiteUrl = contentUrl + "&web=1"
                    }
                }, cancellationToken);
            }

            return (team, createdGroup.DisplayName);
        }

        private async Task<string[]> GetTeamMemberIds(
            GraphServiceClient graphClient,
            Position position,
            User requester,
            CancellationToken cancellationToken)
        {
            var result = new HashSet<string>
            {
                requester.Id
            };

            var hiringManager = position.HiringManager;
            if (hiringManager != null && !string.IsNullOrEmpty(hiringManager.DirectReportIds))
            {
                var ids = hiringManager.DirectReportIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x));

                var members = await _databaseContext.Recruiters
                    .Where(x => ids.Contains(x.RecruiterId))
                    .ToArrayAsync(cancellationToken);

                // because of demo, we don't know user upn and have to build on the flight
                var domain = new MailAddress(requester.UserPrincipalName).Host;
                foreach (var member in members)
                {
                    var upn = $"{member.Alias}@{domain}";
                    var users = await graphClient.Users.Request().Filter($"userPrincipalName eq '{upn}'").GetAsync(cancellationToken);
                    if (users != null && users.Count == 1)
                    {
                        result.Add(users[0].Id);
                    }
                }
            }

            return result.Select(CovertIdToOdataResourceFormat).ToArray();
        }

        private static async Task<string[]> GetTeamOwnerIds(
            GraphServiceClient graphClient,
            Position position,
            User requester,
            CancellationToken cancellationToken)
        {
            var owners = new HashSet<string>
            {
                requester.Id
            };

            var hiringManager = position.HiringManager;
            if (hiringManager != null)
            {
                // because of demo, we don't know user upn and have to build on the flight
                var domain = new MailAddress(requester.UserPrincipalName).Host;
                var upn = $"{hiringManager.Alias}@{domain}";
                var users = await graphClient.Users.Request().Filter($"userPrincipalName eq '{upn}'").GetAsync(cancellationToken);
                if (users != null && users.Count == 1)
                {
                    owners.Add(users[0].Id);
                }
            }

            return owners.Select(CovertIdToOdataResourceFormat).ToArray();
        }

        private static string CovertIdToOdataResourceFormat(string id) => $"https://graph.microsoft.com/v1.0/users/{id}";

        private GraphServiceClient GetGraphServiceClient(string token) => new GraphServiceClient(
               new DelegateAuthenticationProvider(
           requestMessage =>
           {
               requestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, token);
               return Task.CompletedTask;
           }));

        private async Task<string> GetTokenForApp(string tenantId)
        {
            var builder = ConfidentialClientApplicationBuilder.Create(_configuration["MicrosoftAppId"])
                .WithClientSecret(_configuration["MicrosoftAppPassword"])
                .WithTenantId(tenantId)
                .WithRedirectUri("msal" + _configuration["MicrosoftAppId"] + "://auth");

            var client = builder.Build();

            var tokenBuilder = client.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" });

            var result = await tokenBuilder.ExecuteAsync();

            return result.AccessToken;
        }

        private async Task<string> GetOnBehalfOfToken(string token)
        {
            var builder = ConfidentialClientApplicationBuilder.Create(_configuration["MicrosoftAppId"])
                .WithClientSecret(_configuration["MicrosoftAppPassword"]);

            var client = builder.Build();

            //Calls the /oauth2/v2.0/token endpoint to swap the given AAD token for another with different scopes
            var tokenBuilder = client.AcquireTokenOnBehalfOf(new[] { "profile" }, new UserAssertion(token));

            var result = await tokenBuilder.ExecuteAsync();

            return result.AccessToken;
        }
    }
}
