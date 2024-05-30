﻿using System;
using UnityEngine;

namespace RainMeadow.GameModes
{
    public class ArenaClientSettings : ClientSettings
    {
        public class Definition : ClientSettings.Definition
        {
            public Definition() { }

            public Definition(EntityId entityId, OnlinePlayer owner) : base(entityId, owner) { }

            public override OnlineEntity MakeEntity(OnlineResource inResource)
            {
                return new ArenaClientSettings(this);
            }
        }

        public Color bodyColor;
        public Color eyeColor;
        public SlugcatStats.Name playingAs;
        public bool inGame;

        public ArenaClientSettings(Definition entityDefinition) : base(entityDefinition)
        {
            RainMeadow.Debug(this);
            bodyColor = entityDefinition.owner == 2 ? Color.white : PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.White);
            eyeColor = Color.black;
            playingAs = SlugcatStats.Name.White;
            

        }

        internal override AvatarCustomization MakeCustomization()
        {
            return new ArenaAvatarCustomization(this);
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
            public bool inGame;

            public State() { }
            public State(ArenaClientSettings onlineEntity, OnlineResource inResource, uint ts) : base(onlineEntity, inResource, ts)
            {
                bodyColor = onlineEntity.bodyColor;
                eyeColor = onlineEntity.eyeColor;
                playingAs = onlineEntity.playingAs;
                inGame = onlineEntity.inGame;

            }

            public override void ReadTo(OnlineEntity onlineEntity)
            {
                base.ReadTo(onlineEntity);
                var avatarSettings = (ArenaClientSettings)onlineEntity;
                avatarSettings.bodyColor = bodyColor;
                avatarSettings.eyeColor = eyeColor;
                avatarSettings.playingAs = playingAs;
                avatarSettings.inGame = inGame;

            }
        }
        public class ArenaAvatarCustomization : AvatarCustomization
        {
            public readonly ArenaClientSettings settings;

            public ArenaAvatarCustomization(ArenaClientSettings slugcatAvatarSettings)
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