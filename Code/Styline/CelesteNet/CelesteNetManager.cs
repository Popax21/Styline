using System;
using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;

using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.Client.Entities;
using Celeste.Mod.Procedurline;

namespace Celeste.Mod.Styline {
    public class CelesteNetSyncComponent : CelesteNetGameComponent {
        public const int CelesteNetDataVersion = 0;

        private CelesteNetClientContext clientCtx;
        private ConcurrentDictionary<DataPlayerInfo, PlayerProcessor> ghostProcessors = new ConcurrentDictionary<DataPlayerInfo, PlayerProcessor>();

        public CelesteNetSyncComponent(CelesteNetClientContext ctx, Game game) : base(ctx, game) {
            clientCtx = ctx;

            //Add attribute hook
            StylineModule.PlayerProcessor.OnInvalidate += AttributesChangedHandler;
        }

        protected override void Dispose(bool disposing) {
            //Destroy processors
            foreach(PlayerProcessor processor in ghostProcessors.Values) processor.Dispose();
            ghostProcessors.Clear();

            //Remove attribute hook
            StylineModule.PlayerProcessor.OnInvalidate -= AttributesChangedHandler;
    
            base.Dispose(disposing);
        }

        private void AttributesChangedHandler(IScopedInvalidatable scope) {
            //Send current attributes
            IPlayerAttributes attrs = StylineModule.PlayerAttributes;
            if(clientCtx.Client != null && attrs != null) clientCtx.Client.Send(new DataPlayerAttributes(clientCtx.Client.PlayerInfo, attrs));
        }

        public void Handle(CelesteNetConnection con, DataReady data) {
            //Send current attributes
            IPlayerAttributes attrs = StylineModule.PlayerAttributes;
            if(clientCtx.Client != null && attrs != null) {
                clientCtx.Client.Send(new DataPlayerAttributes(clientCtx.Client.PlayerInfo, attrs));
            }
        }

        public void Handle(CelesteNetConnection con, DataPlayerInfo data) {
            if(!string.IsNullOrEmpty(data.DisplayName)) return;

            MainThreadHelper.Do(() => {
                //Dispose processor
                if(!ghostProcessors.TryRemove(data, out PlayerProcessor processor)) return;
                processor.Dispose();
            });
        }

        public void Handle(CelesteNetConnection con, DataPlayerAttributes data) {
            MainThreadHelper.Do(() => {
                //Get and update processor
                PlayerProcessor processor = ghostProcessors.GetOrAdd(data.Player, _ => {
                    return new PlayerProcessor($"STYLINE GHOST {data.Player.ID}", e => e is Ghost ghost && ghost.PlayerInfo == data.Player, false);
                });
                processor.Attributes = data;
            });
        }
    }
}