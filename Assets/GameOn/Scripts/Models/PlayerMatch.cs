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

public class PlayerMatch
{
    public int attemptsRemaining; // optional
    public string creatorPlayerName;
    public long dateEnd;
    public long dateStart;
    public string imageUrl; // optional
    public string matchId;
    public int playerAttemptsPerMatch;
    public int playersPerMatch;
    public string scoreType;
    public string subtitle; // optional
    public string title;
    public string tournamentId;
    public string winType;
    public string tournamentState;
}