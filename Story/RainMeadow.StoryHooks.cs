﻿using UnityEngine;
using System.Collections.Generic;
using HUD;
using System.Linq;
using System;

namespace RainMeadow
{
    public partial class RainMeadow
    {
        private bool isPlayerReady = false;
        public static bool isStoryMode(out StoryGameMode gameMode)
        {
            gameMode = null;
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode sgm)
            {
                gameMode = sgm;
                return true;
            }
            return false;
        }

        private void StoryHooks()
        {
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState;
            On.Menu.SleepAndDeathScreen.ctor += SleepAndDeathScreen_ctor;
            On.Menu.SleepAndDeathScreen.Update += SleepAndDeathScreen_Update;
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;


            On.Menu.KarmaLadderScreen.Singal += KarmaLadderScreen_Singal;

            On.Player.Update += Player_Update;

            On.Player.GetInitialSlugcatClass += Player_GetInitialSlugcatClass;

            On.SlugcatStats.SlugcatFoodMeter += SlugcatStats_SlugcatFoodMeter;
            On.ShortcutGraphics.ShortCutColor += ShortcutGraphics_ShortCutColor;


            On.RegionGate.AllPlayersThroughToOtherSide += RegionGate_AllPlayersThroughToOtherSide;
            On.RegionGate.PlayersStandingStill += PlayersStandingStill;
            On.RegionGate.PlayersInZone += RegionGate_PlayersInZone;

            On.RainWorldGame.GameOver += RainWorldGame_GameOver;
            On.RainWorldGame.GoToDeathScreen += RainWorldGame_GoToDeathScreen;

        }

        private Color ShortcutGraphics_ShortCutColor(On.ShortcutGraphics.orig_ShortCutColor orig, ShortcutGraphics self, Creature crit, RWCustom.IntVector2 pos)
        {

            if (isStoryMode(out var storyGameMode))
            {
                if (crit.Template.type == CreatureTemplate.Type.Slugcat)
                {
                    return (storyGameMode.clientSettings as StoryClientSettings).bodyColor; // TODO: This returns the user's slugcat for all slugcats from their perspective atm. 
                }
                return orig(self, crit, pos);
            }
            return orig(self, crit, pos);

        }

        private void Player_GetInitialSlugcatClass(On.Player.orig_GetInitialSlugcatClass orig, Player self)
        {
            orig(self);
            if (isStoryMode(out var storyGameMode))
            {
                SlugcatStats.Name slugcatClass;
                if ((storyGameMode.clientSettings as StoryClientSettings).playingAs == Ext_SlugcatStatsName.OnlineStoryWhite)
                {
                    self.SlugCatClass = SlugcatStats.Name.White;
                }
                else if ((storyGameMode.clientSettings as StoryClientSettings).playingAs == Ext_SlugcatStatsName.OnlineStoryYellow)
                {
                    self.SlugCatClass = SlugcatStats.Name.Yellow;
                }
                else if ((storyGameMode.clientSettings as StoryClientSettings).playingAs == Ext_SlugcatStatsName.OnlineStoryRed)
                {
                    self.SlugCatClass = SlugcatStats.Name.Red;
                }
            }
        }

        private RWCustom.IntVector2 SlugcatStats_SlugcatFoodMeter(On.SlugcatStats.orig_SlugcatFoodMeter orig, SlugcatStats.Name slugcat)
        {
            if (isStoryMode(out var storyGameMode))
            {
                if (storyGameMode.currentCampaign == Ext_SlugcatStatsName.OnlineStoryWhite)
                {
                    return new RWCustom.IntVector2(7, 4);
                }
                if (storyGameMode.currentCampaign == Ext_SlugcatStatsName.OnlineStoryYellow)
                {
                    return new RWCustom.IntVector2(5, 3);
                }
                if (storyGameMode.currentCampaign == Ext_SlugcatStatsName.OnlineStoryRed)
                {
                    return new RWCustom.IntVector2(9, 6);
                }
            }
            return orig(slugcat);

        }

        private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);
            if (isStoryMode(out var gameMode))
            {
                self.AddPart(new OnlineStoryHud(self, cam, gameMode));
            }
        }

        private void RainWorldGame_GoToDeathScreen(On.RainWorldGame.orig_GoToDeathScreen orig, RainWorldGame self)
        {
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode)
            {
                if (!OnlineManager.lobby.isOwner)
                {
                    OnlineManager.lobby.owner.InvokeRPC(RPCs.MovePlayersToDeathScreen);
                }
                else
                {
                    RPCs.MovePlayersToDeathScreen();
                }
            }
            else
            {
                orig(self);
            }
        }

        private void RainWorldGame_GameOver(On.RainWorldGame.orig_GameOver orig, RainWorldGame self, Creature.Grasp dependentOnGrasp)
        {
            if (isStoryMode(out var gameMode))
            {
                foreach (var playerAvatar in OnlineManager.lobby.playerAvatars.Values)
                {

                    if (playerAvatar.type == (byte)OnlineEntity.EntityId.IdType.none) continue; // not in game
                    if (playerAvatar.FindEntity(true) is OnlinePhysicalObject opo && opo.apo is AbstractCreature ac)
                    {
                        if (ac.state.alive) return;
                    }
                }
                //INITIATE DEATH
                foreach (OnlinePlayer player in OnlineManager.players)
                {
                    if (!player.isMe)
                    {
                        player.InvokeRPC(RPCs.InitGameOver);
                    }
                    else
                    {
                        orig(self, dependentOnGrasp);
                    }
                }
            }
            else
            {
                orig(self, dependentOnGrasp);
            }
        }

        private SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            var origSaveState = orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
            if (isStoryMode(out var gameMode))
            {
                //self.currentSaveState.LoadGame(gameMode.saveStateProgressString, game); //pretty sure we can just stuff the string here
                var storyClientSettings = gameMode.clientSettings as StoryClientSettings;
                origSaveState.denPosition = storyClientSettings.myLastDenPos;
                return origSaveState;
            }
            return origSaveState;
        }

        private void KarmaLadderScreen_Singal(On.Menu.KarmaLadderScreen.orig_Singal orig, Menu.KarmaLadderScreen self, Menu.MenuObject sender, string message)
        {
            if (isStoryMode(out var gameMode))
            {
                if (message == "CONTINUE")
                {
                    if (OnlineManager.lobby.isOwner)
                    {
                        gameMode.didStartCycle = true;
                    }
                }
            }
            orig(self, sender, message);
        }

        //On Static hook class




        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (isStoryMode(out var gameMode))
            {

                //fetch the online entity and check if it is mine. 
                //If it is mine run the below code
                //If not, update from the lobby state
                //self.readyForWin = OnlineMAnager.lobby.playerid === fetch if this is ours. 

                if (OnlinePhysicalObject.map.TryGetValue(self.abstractCreature, out var oe))
                {
                    if (!oe.isMine)
                    {
                        self.readyForWin = gameMode.readyForWinPlayers.Contains(oe.owner.inLobbyId);
                        return;
                    }
                }

                if (self.readyForWin
                    && self.touchedNoInputCounter > (ModManager.MMF ? 40 : 20)
                    && RWCustom.Custom.ManhattanDistance(self.abstractCreature.pos.Tile, self.room.shortcuts[0].StartTile) > 3)
                {
                    gameMode.storyClientSettings.readyForWin = true;
                }
                else
                {
                    gameMode.storyClientSettings.readyForWin = false;
                }
            }
        }

        private void SleepAndDeathScreen_Update(On.Menu.SleepAndDeathScreen.orig_Update orig, Menu.SleepAndDeathScreen self)
        {
            orig(self);

            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode storyGameMode)
            {
                self.continueButton.buttonBehav.greyedOut = !isPlayerReady;
            }
        }

        private void SleepAndDeathScreen_ctor(On.Menu.SleepAndDeathScreen.orig_ctor orig, Menu.SleepAndDeathScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {

            RainMeadow.Debug("In SleepAndDeath Screen");
            orig(self, manager, ID);

            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode storyGameMode)
            {
                isPlayerReady = false;
                storyGameMode.didStartCycle = false;
                //Create the READY button
                var buttonPosX = self.ContinueAndExitButtonsXPos - 180f - self.manager.rainWorld.options.SafeScreenOffset.x;
                var buttonPosY = Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 53f);
                var readyButton = new SimplerButton(self, self.pages[0], "READY",
                    new Vector2(buttonPosX, buttonPosY),
                    new Vector2(110f, 30f));

                readyButton.OnClick += ReadyButton_OnClick;

                self.pages[0].subObjects.Add(readyButton);
                readyButton.black = 0;
                self.pages[0].lastSelectedObject = readyButton;

            }
        }

        private void ReadyButton_OnClick(SimplerButton obj)
        {
            if ((isStoryMode(out var gameMode) && gameMode.didStartCycle == true) || OnlineManager.lobby.isOwner)
            {
                isPlayerReady = true;
            }
        }

        private bool RegionGate_AllPlayersThroughToOtherSide(On.RegionGate.orig_AllPlayersThroughToOtherSide orig, RegionGate self)
        {

            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode)
            {
                foreach (var playerAvatar in OnlineManager.lobby.playerAvatars.Values)
                {
                    if (playerAvatar.type == (byte)OnlineEntity.EntityId.IdType.none) continue; // not in game
                    if (playerAvatar.FindEntity(true) is OnlinePhysicalObject opo && opo.apo is AbstractCreature ac)
                    {
                        if (ac.pos.room == self.room.abstractRoom.index && (!self.letThroughDir || ac.pos.x < self.room.TileWidth / 2 + 3)
                            && (self.letThroughDir || ac.pos.x > self.room.TileWidth / 2 - 4))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // not loaded
                    }
                }
                return true;
            }
            return orig(self);

        }


        private int RegionGate_PlayersInZone(On.RegionGate.orig_PlayersInZone orig, RegionGate self)
        {
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode)
            {
                int regionGateZone = -1;
                foreach (var playerAvatar in OnlineManager.lobby.playerAvatars.Values)
                {
                    if (playerAvatar.type == (byte)OnlineEntity.EntityId.IdType.none) continue; // not in game
                    if (playerAvatar.FindEntity(true) is OnlinePhysicalObject opo && opo.apo is AbstractCreature ac)
                    {
                        if (ac.Room == self.room.abstractRoom)
                        {
                            int zone = self.DetectZone(ac);
                            if (zone != regionGateZone && regionGateZone != -1)
                            {
                                return -1;
                            }
                            regionGateZone = zone;
                        }
                    }
                    else
                    {
                        return -1; // not loaded
                    }
                }

                return regionGateZone;
            }
            return orig(self);
        }

        private bool PlayersStandingStill(On.RegionGate.orig_PlayersStandingStill orig, RegionGate self)
        {
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is StoryGameMode)
            {
                foreach (var playerAvatar in OnlineManager.lobby.playerAvatars.Values)
                {
                    if (playerAvatar.type == (byte)OnlineEntity.EntityId.IdType.none) continue; // not in game
                    if (playerAvatar.FindEntity(true) is OnlinePhysicalObject opo && opo.apo is AbstractCreature ac)
                    {
                        if (ac.Room != self.room.abstractRoom
                        || ((ac.realizedCreature as Player)?.touchedNoInputCounter ?? 0) < (ModManager.MMF ? 40 : 20))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // not loaded
                    }
                }
                return true;
            }
            return orig(self);
        }
    }
}






