using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Refit;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Models.MicrosoftGraph;
using TeamsTalentMgmtAppV3.Services.Data;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamsTalentMgmtAppV3.Services.Refit;

namespace TeamsTalentMgmtAppV3.Services
{
    public sealed class GraphApiService : IGraphApiService
    {
        private readonly DatabaseContext _databaseContext;

        public GraphApiService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<Team> CreateNewTeamForPosition(Position position, string token)
        {
            var graphClient = BuildGraphApiClient(token);
            // If you have a user's UPN, you can add it directly to a group, but then there will be a 
            // significant delay before Microsoft Teams reflects the change. Instead, we find the user 
            // object's id, and add the ID to the group through the Graph beta endpoint, which is 
            // recognized by Microsoft Teams much more quickly. See 
            // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/teams_api_overview 
            // for more about delays with adding members.
            var requester = await graphClient.Me();
            var ownerIds = await GetTeamOwnerIds(graphClient, position, requester);
            var memberIds = await GetTeamMemberIds(graphClient, position, requester);


            // In order to create a team, the group must have a least one owner.
            var group = await CreateGroup(graphClient, position.PositionExternalId, ownerIds, memberIds);

            // If the group was created less than 15 minutes ago, it's possible for the Create team call to fail with a 404 error code due to replication delays.
            var team = await CreateTeam(graphClient, group.Id);

            var channel = await CreateChannel(graphClient, team.Id);

            var teamsAppIdFromManifest = ConfigurationManager.AppSettings["TeamsAppId"];
            await AddAppToTeam(graphClient, team.Id, channel.Id, teamsAppIdFromManifest, position);

            team.DisplayName = group.DisplayName;
            return team;
        }

        private static async Task AddAppToTeam(IGraphRestApiService graphClient, string teamId, string channelId, string teamsAppId, Position position)
        {
            var teamsApps = await graphClient.ListTeamsApps(teamsAppId);
            var teamApp = teamsApps.Items.FirstOrDefault();
            if (!string.IsNullOrEmpty(teamApp?.Id))
            {
                var appId = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{teamApp.Id}";
                await graphClient.AddAppToTeam(teamId, new TeamsApp
                {
                    TeamsAppId = appId
                });

                var contentUrl = $"{ConfigurationManager.AppSettings["BaseUrl"]}/StaticViews/TeamTab.html?positionId={position.PositionId}";
                await graphClient.AddTab(teamId, channelId, new TeamsTab
                {
                    TeamsAppId = appId,
                    DisplayName = position.PositionExternalId,
                    Configuration = new TeamsTabConfiguration
                    {
                        EntityId = position.PositionId.ToString(),
                        ContentUrl = contentUrl,
                        WebsiteUrl = contentUrl + "&web=1"
                    }
                });
            }
        }

        private static Task<Channel> CreateChannel(IGraphRestApiService graphClient, string teamId) =>
            graphClient.CreateChannelForTeam(teamId, new Channel
            {
                DisplayName = "Candidates",
                Description = "Discussion about interview, feedback, etc."
            });

        private static Task<Team> CreateTeam(IGraphRestApiService graphClient, string groupId) =>
            graphClient.CreateTeamForGroup(groupId, new Team
            {
                GuestSettings = new TeamPerRoleSettings
                {
                    AllowCreateUpdateChannels = false,
                    AllowDeleteChannels = false
                },
                MemberSettings = new TeamPerRoleSettings
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
                    GiphyContentRating = "strict"
                }
            });

        private static Task<Group> CreateGroup(IGraphRestApiService graphClient, string positionId, string[] ownerIds, string[] memberIds) =>
            graphClient.CreateGroup(new Group
            {
                DisplayName = $"Position {positionId}",
                MailNickname = positionId,
                Description = $"Everything about position {positionId}",
                Visibility = "Private",
                Owners = ownerIds,
                Members = memberIds,
                GroupTypes = new[] { "Unified" }, // Office 365 (aka unified group)
                MailEnabled = true, // true if creating an Office 365 Group
                SecurityEnabled = false // false if creating an Office 365 group
            });

        private static string CovertIdToOdataResourceFormat(string id) => $"https://graph.microsoft.com/v1.0/users/{id}";

        private async Task<string[]> GetTeamMemberIds(IGraphRestApiService graphClient, Position position, User requester)
        {
            var result = new HashSet<string>
            {
                requester.Id
            };
            var hiringManager = position.HiringManager;
            if (hiringManager != null && hiringManager.DirectReportIds.HasValue())
            {
                var ids = hiringManager.DirectReportIds
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x));

                var members = await _databaseContext.Recruiters
                    .Where(x => ids.Contains(x.RecruiterId))
                    .ToListAsync();

                // because of demo, we don't know user upn and have to build on the flight
                var domain = new MailAddress(requester.UserPrincipalName).Host;
                foreach (var member in members)
                {
                    var upn = $"{member.Alias}@{domain}";
                    var user = await graphClient.GetUserByUpn(upn);
                    if (user != null)
                    {
                        result.Add(user.Id);
                    }
                }
            }

            return result.Select(CovertIdToOdataResourceFormat).ToArray();
        }

        private static async Task<string[]> GetTeamOwnerIds(IGraphRestApiService graphClient, Position position, User requester)
        {
            var result = new HashSet<string>
            {
                requester.Id
            };

            var hiringManager = position.HiringManager;
            if (hiringManager != null)
            {
                // because of demo, we don't know user upn and have to build on the flight
                var domain = new MailAddress(requester.UserPrincipalName).Host;
                var upn = $"{hiringManager.Alias}@{domain}";
                var user = await graphClient.GetUserByUpn(upn);
                if (user != null)
                {
                    result.Add(user.Id);
                }
            }

            return result.Select(CovertIdToOdataResourceFormat).ToArray();
        }

        private static IGraphRestApiService BuildGraphApiClient(string token) => RestService.For<IGraphRestApiService>(
            new HttpClient(new AuthenticatedHttpClientHandler(token))
            {
                BaseAddress = new Uri("https://graph.microsoft.com")
            });
    }
}