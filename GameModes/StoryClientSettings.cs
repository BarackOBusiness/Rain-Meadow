﻿using System;
using UnityEngine;

namespace RainMeadow
{
    public class StoryClientSettings : ClientSettings
    {
        public class Definition : ClientSettings.Definition
        {
            public Definition() { }

            public Definition(EntityId entityId, OnlinePlayer owner) : base(entityId, owner) { }

            public override OnlineEntity MakeEntity(OnlineResource inResource)
            {
                return new StoryClientSettings(this);
            }
        }

        public Color bodyColor;
        public Color eyeColor; // unused
        public SlugcatStats.Name playingAs;
        public bool readyForWin;
        public string myLastDenPos;
        public bool inGame;
        public bool isDead;

        public StoryClientSettings(Definition entityDefinition) : base(entityDefinition)
        {
            RainMeadow.Debug(this);
            playingAs = RainMeadow.Ext_SlugcatStatsName.OnlineStoryWhite; //this is bad, we'll need to investigate this.
            myLastDenPos = "";
            bodyColor = entityDefinition.owner == 2 ? Color.cyan : PlayerGraphics.DefaultSlugcatColor(playingAs);
            eyeColor = Color.black;
        }

        internal override AvatarCustomization MakeCustomization()
        {
            return new SlugcatCustomization(this);
        }

        protected override EntityState MakeState(uint tick, OnlineResource inResource)
        {
            return new State(this, inResource, tick);
        }

        internal Color SlugcatColor()
        {
            return bodyColor;
        }

        public class State : ClientSettings.State
        {
            [OnlineFieldColorRgb]
            public Color bodyColor;
            [OnlineFieldColorRgb]
            public Color eyeColor;
            [OnlineField]
            public SlugcatStats.Name playingAs;
            [OnlineField(group = "game")]
            public bool readyForWin;
            [OnlineField(group = "game")]
            public bool inGame;
            [OnlineField(group = "game")]
            public bool isDead;

            public State() { }
            public State(StoryClientSettings onlineEntity, OnlineResource inResource, uint ts) : base(onlineEntity, inResource, ts)
            {
                bodyColor = onlineEntity.bodyColor;
                eyeColor = onlineEntity.eyeColor;
                playingAs = onlineEntity.playingAs;
                readyForWin = onlineEntity.readyForWin;
                inGame = onlineEntity.inGame;
                isDead = onlineEntity.isDead;
            }

            public override void ReadTo(OnlineEntity onlineEntity)
            {
                base.ReadTo(onlineEntity);
                var avatarSettings = (StoryClientSettings)onlineEntity;
                avatarSettings.bodyColor = bodyColor;
                avatarSettings.eyeColor = eyeColor;
                avatarSettings.playingAs = playingAs;
                avatarSettings.readyForWin = readyForWin;
                avatarSettings.inGame = inGame;
                avatarSettings.isDead = isDead;
            }
        }
        public class SlugcatCustomization : AvatarCustomization
        {
            public readonly StoryClientSettings settings;

            public SlugcatCustomization(StoryClientSettings slugcatAvatarSettings)
            {
                this.settings = slugcatAvatarSettings;
            }

            internal override void ModifyBodyColor(ref Color bodyColor)
            {
                bodyColor = new Color(Mathf.Clamp(settings.bodyColor.r, 0.004f, 0.996f), Mathf.Clamp(settings.bodyColor.g, 0.004f, 0.996f), Mathf.Clamp(settings.bodyColor.b, 0.004f, 0.996f));
            }

            internal override void ModifyEyeColor(ref Color eyeColor)
            {
                eyeColor = new Color(Mathf.Clamp(settings.eyeColor.r, 0.004f, 0.996f), Mathf.Clamp(settings.eyeColor.g, 0.004f, 0.996f), Mathf.Clamp(settings.eyeColor.b, 0.004f, 0.996f));
            }
        }
    }
}