/*
 * Copyright 2019, Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 *  
 * or in the "license" file accompanying this file. This file is distributed 
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either 
 * express or implied. See the License for the specific language governing 
 * permissions and limitations under the License.
 *   
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameOnAPI : ServiceAPI
{
    public string sessionId { get; private set; }
    public long sessionExpirationDate { get; private set; }

    public IEnumerator RegisterPlayer(Action<RegisterPlayerResponse> onRegisterPlayerResponse, 
                                      Action<string> onError)
    {
        RegisterPlayerRequest request = new RegisterPlayerRequest();
        yield return SyncPost(
            request,
            responseText =>
            {
                onRegisterPlayerResponse(RegisterPlayerResponse.FromJSON(responseText));
            },
            onError
        );
    }


    public IEnumerator AuthPlayer(string playerToken, 
                                  string playerName,
                                  Action<AuthPlayerResponse> onAuthPlayerResponse,
                                  Action<string> onError)
    {
        if (String.IsNullOrEmpty(playerToken)) {
            throw new Exception("playerToken required but is null or empty");
        }

        AuthPlayerRequest request = new AuthPlayerRequest
        {
            appBuildType = "development",
            deviceOSType = "android",
            playerToken = playerToken
        };

        // only set player name is one was provided
        if (!string.IsNullOrEmpty(playerName)) 
        {
            request.playerName = playerName;
        }

        yield return SyncPost(
            request,
            responseText =>
            {
                var authPlayerResponse = AuthPlayerResponse.FromJSON(responseText);
                sessionId = authPlayerResponse.sessionId;
                sessionExpirationDate = authPlayerResponse.sessionExpirationDate;
                onAuthPlayerResponse(authPlayerResponse);
            },
            onError
        );
    }

    public IEnumerator GetTournamentList(GetTournamentListRequest request,
                                         Action<GetTournamentListResponse> onTournamentListResponse,
                                         Action<string> onError)
    {
        yield return SyncGet(
            request,
            responseText =>
            {
                if (!string.IsNullOrEmpty(responseText))
                {
                    onTournamentListResponse(GetTournamentListResponse.FromJSON(responseText));
                } 
                else
                {
                    onTournamentListResponse(new GetTournamentListResponse() { tournaments = new List<Tournament>() });
                }
            },
            onError
        );
    }

    public IEnumerator GetMatchList(GetMatchListRequest request,
                                    Action<GetMatchListResponse> onMatchListResponse,
                                    Action<string> onError)
    {
        yield return SyncGet(
            request,
            responseText =>
            {
                if (!string.IsNullOrEmpty(responseText))
                {
                    onMatchListResponse(GetMatchListResponse.FromJSON(responseText)); 
                }
                else
                {
                    onMatchListResponse(new GetMatchListResponse() { matches = new List<Match>() });
                }
            },
            onError
        );
    }

    public IEnumerator EnterTournament(string tournamentId,
                                       Action<EnterTournamentResponse> onEnterTournamentResponse,
                                       Action<string> onError)
    {
        yield return SyncPost(
            new EnterTournamentRequest(tournamentId),
            responseText =>
            {
                onEnterTournamentResponse(EnterTournamentResponse.FromJSON(responseText));
            },
            onError
        );
    }

    public IEnumerator EnterMatch(string matchId,
                                  Action<EnterMatchResponse> onEnterMatchResponse,
                                  Action<string> onError)
    {
        yield return SyncPost(
            new EnterMatchRequest(matchId),
            responseText =>
            {
                onEnterMatchResponse(EnterMatchResponse.FromJSON(responseText));
            },
            onError
        );
    }

    public IEnumerator SubmitScore(string matchId,
                                   int score,
                                   Action<SubmitScoreResponse> onSubmitScoreResponse,
                                   Action<string> onError)
    {
        SubmitScoreRequest request = new SubmitScoreRequest(matchId)
        {
            score = score
        };

        yield return SyncPut(
            request,
            responseText =>
            {
                onSubmitScoreResponse(SubmitScoreResponse.FromJSON(responseText));
            },
            onError
        );
    }

    public IEnumerator GetLeaderboard(string matchId,
                                      int limit,
                                      Action<GetMatchLeaderboardResponse> onMatchLeaderboardResponse,
                                      Action<string> onError)
    {
        GetMatchLeaderboardRequest request = new GetMatchLeaderboardRequest(matchId)
        {
            limit = limit
        };

        yield return SyncGet(
            request,
            responseText =>
            {
                print("Learderboard Response: " + responseText);
                onMatchLeaderboardResponse(GetMatchLeaderboardResponse.FromJSON(responseText));
            },
            onError
        );
    }

    public IEnumerator GetPlayerRank(string matchId,
                                     int currentPlayerNeighbors,
                                     int limit,
                                     Action<GetMatchLeaderboardResponse> onMatchLeaderboardResponse,
                                     Action<string> onError)
    {
        GetPlayerRankRequest request = new GetPlayerRankRequest(matchId)
        {
            currentPlayerNeighbors = currentPlayerNeighbors,
            limit = limit
        };

        yield return SyncGet(
            request,
            responseText =>
            {
                print("Learderboard Response: " + responseText);
                onMatchLeaderboardResponse(GetMatchLeaderboardResponse.FromJSON(responseText));
            },
            onError
        );
    }

    public IEnumerator GetMatchDetails(string matchId,
                                       Action<GetMatchDetailsResponse> onMatchDetailsResponse,
                                       Action<string> onError)
    {
        yield return SyncGet(
            new GetMatchDetailsRequest(matchId),
            responseText =>
            {
                onMatchDetailsResponse(GetMatchDetailsResponse.FromJSON(responseText));
            },
            onError
        );
    }

    public IEnumerator ClaimPrizes(string[] prizeIds,
                                   Action<ClaimPrizeListResponse> onClaimPrizeListResponse,
                                   Action<string> onError)
    {
        ClaimPrizeListRequest request = new ClaimPrizeListRequest
        {
            awardedPrizeIds = new List<string>(prizeIds)
        };

        yield return SyncPost(
            request,
            responseText =>
            {
                onClaimPrizeListResponse(ClaimPrizeListResponse.FromJSON(responseText));
            },
            onError
        );
    }

    #region Protected Overrides
    protected override void AddHeaders(UnityWebRequest request)
    {
        base.AddHeaders(request);

        if (!String.IsNullOrEmpty(sessionId))
        {
            request.SetRequestHeader("session-id", sessionId);
        }
    }
    #endregion
}
