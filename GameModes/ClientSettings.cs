﻿using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using static RainMeadow.OnlineState;

namespace RainMeadow
{
    public abstract class ClientSettings : OnlineEntity
    {
        public abstract class Definition : EntityDefinition
        {
            public Definition() : base() { }
            public Definition(OnlineEntity.EntityId entityId, OnlinePlayer owner) : base(entityId, owner, false) { }
        }

        /// <summary>
        /// the real avatar of the player
        /// </summary>
        public OnlineEntity.EntityId avatarId;
        public bool inGame;
        public ClientSettings(EntityDefinition entityDefinition) : base(entityDefinition)
        {
            avatarId = new OnlineEntity.EntityId(entityDefinition.owner, OnlineEntity.EntityId.IdType.none, 0);
        }

        internal abstract AvatarCustomization MakeCustomization();

        public abstract class AvatarCustomization
        {
            internal abstract void ModifyBodyColor(ref Color bodyColor);

            internal abstract void ModifyEyeColor(ref Color eyeColor);
        }

        public abstract class State : EntityState
        {
            [OnlineField(nullable:true)]
            private EntityId avatarId;
            [OnlineField(group = "game")]
            private bool inGame;

            protected State() { }

            protected State(ClientSettings clientSettings, OnlineResource inResource, uint ts) : base(clientSettings, inResource, ts)
            {
                this.avatarId = clientSettings.avatarId;
                this.inGame = clientSettings.inGame;

            }

            public override void ReadTo(OnlineEntity onlineEntity)
            {
                base.ReadTo(onlineEntity);
                (onlineEntity as ClientSettings).avatarId = avatarId;
                (onlineEntity as ClientSettings).inGame = inGame;

            }
        }
    }
}