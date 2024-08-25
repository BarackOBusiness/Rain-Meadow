﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RainMeadow
{
    internal class ArenaLobbyData : OnlineResource.ResourceData
    {
        public ArenaLobbyData(OnlineResource resource) : base(resource) { }

        internal override ResourceDataState MakeState()
        {
            return new State(this, resource);
        }

        internal class State : ResourceDataState
        {
            [OnlineField]
            public bool isInGame;
            [OnlineField]
            public bool nextLevel;
            [OnlineField]
            public List<string> playList;
            [OnlineField]
            public int denChoice;


            public State() { }
            public State(ArenaLobbyData arenaLobbyData, OnlineResource onlineResource)
            {
                ArenaCompetitiveGameMode arena = (onlineResource as Lobby).gameMode as ArenaCompetitiveGameMode;
                isInGame = RWCustom.Custom.rainWorld.processManager.currentMainLoop is RainWorldGame;
                nextLevel = arena.nextLevel;
                playList = arena.playList;
                denChoice = arena.denChoice;



            }

            internal override Type GetDataType() => typeof(ArenaLobbyData);

            internal override void ReadTo(OnlineResource.ResourceData data)
            {
                var lobby = (data.resource as Lobby);
                (lobby.gameMode as ArenaCompetitiveGameMode).isInGame = isInGame;

                (lobby.gameMode as ArenaCompetitiveGameMode).playList = playList;
                (lobby.gameMode as ArenaCompetitiveGameMode).nextLevel = nextLevel;
                (lobby.gameMode as ArenaCompetitiveGameMode).denChoice = denChoice;



            }
        }
    }
}