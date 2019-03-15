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
using UnityEngine;

public class GameOn : MonoBehaviour
{
    // Reference to the GameOn API class
    public GameOnAPI gameOnApi;

    public bool authenticated;

    // config
    public static bool debug = true;
    public string PublicApiKey;
    public string PlayerToken;
    public string SessionId { get; private set; }

    public Dictionary<string, double[]> matchScores = new Dictionary<string, double[]>();

    void DebugOutput(string message)
    {
        if (debug)
        {
            Debug.Log("GameOn Debug: " + message);
        }
    }

    void Awake()
    {
        gameOnApi = GetComponent<GameOnAPI>();
    }

    public void AsyncRegisterPlayer()
    {
        StartCoroutine(RegisterPlayer());
    }

    public IEnumerator RegisterPlayer()
    {
        yield return gameOnApi.RegisterPlayer(
            onRegisterPlayerResponse => { 
                PlayerToken = onRegisterPlayerResponse.playerToken;
            },
            null
        );
    }

    public void AsyncAuthPlayer(string playerToken, string playerName)
    {
        StartCoroutine(AuthPlayer(playerToken, playerName));
    }

    public IEnumerator AuthPlayer(string playerToken, string playerName)
    {
        yield return gameOnApi.AuthPlayer(
            playerToken,
            playerName, 
            onAuthPlayerResponse => { authenticated = true; },
            null
        );
    }

    public List<Tournament> tournaments = new List<Tournament>();

    public IEnumerator GetTournamentList()
    {
        GetTournamentListRequest request = new GetTournamentListRequest
        {
            limit = "10",
            playerAttributes = "",
            period = "ALL",
            filterBy = "LIVE"
        };

        yield return gameOnApi.GetTournamentList(
            request,
            response =>
            {
                // clear existing tournaments
                tournaments.Clear();

                DebugOutput("Tournament Get Success: Found " + response.tournaments.Count + " tournament(s)");
                tournaments.AddRange(response.tournaments);
            },
            error =>
            {
                DebugOutput("Tournament Get Error: " + error);
            }
        );
    }

    public IEnumerator EnterTournament(Tournament tournament, Action<string> onMatchId)
    {
        yield return gameOnApi.EnterTournament(tournament.tournamentId,
            response =>
            {
                DebugOutput("Entering Tournament " + tournament.title);

                // refresh the tournament list to avoid un-intentional re-entry
                // (probably not nessisary for a real game)
                StartCoroutine(GetTournamentList());

                // refresh match list, because we should have a new match for 
                // the tournament that was entered 
                // (probably not nessisary for a real game)
                StartCoroutine(GetMatchList());
                onMatchId(response.matchId);
            },
            error =>
            {
                DebugOutput("Error Entering Tournament " + error);
            }
        );
    }

    public Dictionary<string, string> matches = new Dictionary<string, string>();

    public IEnumerator GetMatchList()
    {
        GetMatchListRequest request = new GetMatchListRequest
        {
            filterBy = "live",
            limit = -1, // all
            matchType = "all",
            period = "all"
        };

        yield return gameOnApi.GetMatchList(
            request,
            response =>
            {
                // clear existing matches
                matches.Clear();

                DebugOutput("Match Get Success: Found " + response.matches.Count + " match(es)");
                foreach (Match match in response.matches)
                {
                    DebugOutput("  Found match: \"" + match.title + "\" (" + match.matchId + ")");
                    matches.Add(match.matchId, match.title);
                }

                DebugOutput("Found " + response.playerMatches.Count + " player match(es)");
                foreach (PlayerMatch playerMatch in response.playerMatches)
                {
                    DebugOutput("  Found player match: \"" + playerMatch.title + "\" (" + playerMatch.matchId + ")");
                }
            },
            error =>
            {
                DebugOutput("Error getting match list: " + error);
            }
        );
    }

    public IEnumerator EnterMatch(string matchId, Action<bool> onSuccess)
    {
        yield return gameOnApi.EnterMatch(matchId,
            response =>
            {
                DebugOutput("Entered match " + matchId);
                onSuccess(true);
            },
            error =>
            {
                DebugOutput("Error entering match: " + error);
            }
        );
    }

    public IEnumerator SubmitScoreAndGetLeaderboard(string matchId, int score, int leaderboardLimit, Action<bool> onSuccess)
    {
        yield return gameOnApi.SubmitScore(matchId, score,
            response =>
            {
                DebugOutput("Score Submitted to GameOn " + score);

                StartCoroutine(GetLeaderboard(matchId, leaderboardLimit));
                onSuccess(true);
            },
            error =>
            {
                DebugOutput("Error submitting score: " + error);
            }
        );
    }

    public IEnumerator GetLeaderboard(string matchID, int limit)
    {
        // Set up the container for the scores
        if (!matchScores.ContainsKey(matchID))
        {
            matchScores.Add(matchID, new double[limit]);
        }

        yield return gameOnApi.GetLeaderboard(matchID, limit,
            response =>
            {
                var leaderboardResponse = response;
                var leaderboard = leaderboardResponse.leaderboard;
                var total = leaderboard.Count;

                DebugOutput("Scores for match id " + matchID);
                for (int i = 0; i < total; i++)
                {
                    var matchScore = leaderboard[i];
                    matchScores[matchID][i] = matchScore.score;

                    DebugOutput("Score " + i + " is " + matchScore.score);
                }

            },
            error =>
            {
                DebugOutput("Get Rank Error: " + error);
            }
        );
    }
}
