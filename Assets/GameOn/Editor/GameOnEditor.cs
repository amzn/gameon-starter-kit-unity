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
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameOn))]
public class GameOnEditor : Editor
{
    readonly Dictionary<string, int> submitScoreFieldCache = new Dictionary<string, int>();
    readonly Dictionary<string, bool> hasEnteredMatchCache = new Dictionary<string, bool>();
    readonly Dictionary<string, bool> leaderboardFoldoutCache = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        GameOn goTarget = (GameOn)target;

        GUILayout.Space(15);

        GUILayout.Label("Starter Kit Config", EditorStyles.boldLabel);

        GUILayout.BeginVertical("HelpBox");

        goTarget.PublicApiKey = EditorGUILayout.TextField("Game/Public API Key", goTarget.PublicApiKey);

        goTarget.PlayerToken = EditorGUILayout.TextField("Existing Player Token (optional)", goTarget.PlayerToken);

        GUILayout.EndVertical();

        GUILayout.Space(10);

        if (!Application.isPlaying)
        {
            End();
            return;
        }

        // no known registered player
        if (string.IsNullOrEmpty(goTarget.PlayerToken)) {

            GUILayout.Label("Register Player", EditorStyles.boldLabel);

            if (GUILayout.Button("Register New Player"))
            {
                goTarget.AsyncRegisterPlayer();
            }

            End();
            return;
        }

        GUILayout.Label("Current Player Info", EditorStyles.boldLabel);

        GUILayout.BeginVertical("HelpBox");

        EditorGUILayout.LabelField("Player Token", goTarget.PlayerToken);
        EditorGUILayout.LabelField("Is Authenticated", goTarget.authenticated ? "true" : "false");
        EditorGUILayout.LabelField("Session ID", goTarget.gameOnApi.sessionId);
        EditorGUILayout.LabelField("Session Expiration Date", goTarget.gameOnApi.sessionExpirationDate.ToString());

        GUILayout.EndVertical();

        GUILayout.Space(10);

        if (!goTarget.authenticated)
        {

            GUILayout.Label("Authenticate Player", EditorStyles.boldLabel);

            GUILayout.BeginVertical("HelpBox");

            // set a new name for the player if entered
            string playerName = EditorGUILayout.TextField("Player Name (optional)", "").Trim();

            if (GUILayout.Button("Authenticate Player"))
            {
                goTarget.AsyncAuthPlayer(goTarget.PlayerToken, playerName);
            }

            GUILayout.EndVertical();

            End();
            return;
        }

        GUILayout.Space(5);

        // filter out tournaments the player cannot enter
        var tournaments = goTarget.tournaments.Where(tournament => tournament.canEnter == true);

        EditorGUILayout.LabelField("Available Tournaments (" + tournaments.Count() + ")", EditorStyles.boldLabel);

        GUILayout.Space(5);

        foreach (var tournament in tournaments)
        {
            // only show tournaments players can enter
            if (!tournament.canEnter) {
                continue;
            }

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Tournament Title", tournament.title);
            EditorGUILayout.LabelField("Tournament ID", tournament.tournamentId);

            GUILayout.Space(5);

            if (GUILayout.Button("Enter Tournament"))
            {
                goTarget.StartCoroutine(goTarget.EnterTournament(tournament, matchId => {
                    hasEnteredMatchCache[matchId] = true;
                }));
            }

            GUILayout.Space(5);

            GUILayout.EndVertical();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Get Tournament List"))
        {
            goTarget.StartCoroutine(goTarget.GetTournamentList());
        }

        GUILayout.Space(15);

        var matches = goTarget.matches;

        EditorGUILayout.LabelField("Live Matches (" + matches.Count + ")", EditorStyles.boldLabel);

        GUILayout.Space(5);

        foreach (var match in matches)
        {
            var tournamentTitle = match.Value;
            var matchId = match.Key;

            GUILayout.BeginVertical("HelpBox");

            EditorGUILayout.LabelField("Tournament Title", tournamentTitle);
            EditorGUILayout.LabelField("Match ID", matchId);

            GUILayout.BeginHorizontal();

            if (hasEnteredMatchCache.ContainsKey(matchId) && hasEnteredMatchCache[matchId] == true)
            {
                if (!submitScoreFieldCache.ContainsKey(matchId))
                    submitScoreFieldCache.Add(matchId, 0);

                submitScoreFieldCache[matchId] = EditorGUILayout.IntField("Submit Score", submitScoreFieldCache[matchId]);

                if (GUILayout.Button("Submit Score"))
                {
                    goTarget.StartCoroutine(
                        goTarget.SubmitScoreAndGetLeaderboard(
                            matchId, 
                            submitScoreFieldCache[matchId], 
                            10, 
                            onSuccess => { hasEnteredMatchCache[matchId] = false; }));
                    submitScoreFieldCache[matchId] = 0;
                }
            }
            else
            {
                if (GUILayout.Button("Enter Match"))
                {
                    goTarget.StartCoroutine(goTarget.EnterMatch(matchId, onSuccess => { hasEnteredMatchCache[matchId] = true; }));
                    submitScoreFieldCache[matchId] = 0;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (!leaderboardFoldoutCache.ContainsKey(matchId))
            {
                leaderboardFoldoutCache.Add(matchId, false);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(10)); // pad foldout inside vertical
            leaderboardFoldoutCache[matchId] = EditorGUILayout.Foldout(leaderboardFoldoutCache[matchId], "Top 10 scores");
            GUILayout.EndHorizontal();

            if (leaderboardFoldoutCache[matchId])
            {
                if (!goTarget.matchScores.ContainsKey(matchId))
                {
                    goTarget.StartCoroutine(goTarget.GetLeaderboard(matchId, 10));
                }

                var total = goTarget.matchScores[matchId].Length;

                for (int i = 0; i < total; i++)
                {
                    GUILayout.Label((i + 1) + ") " + goTarget.matchScores[matchId][i]);
                }

                GUILayout.Space(2);
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Get Match List"))
        {
            goTarget.matches.Clear();
            goTarget.StartCoroutine(goTarget.GetMatchList());
            submitScoreFieldCache.Clear();
        }

        End();
    }

    void End() {
        GUILayout.Space(15);
        Repaint();
    }
}
