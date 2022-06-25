using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Monocle;

namespace Celeste.Mod.Styline {
    public class HairAccessory : IDisposable {
        private static readonly LinkedList<HairAccessory> accessoryList = new LinkedList<HairAccessory>();

        public static void Load() => On.Celeste.PlayerHair.Render += HairRenderHook;
        public static void Unload() => On.Celeste.PlayerHair.Render -= HairRenderHook;

        private static void HairRenderHook(On.Celeste.PlayerHair.orig_Render orig, PlayerHair hair) {
            orig(hair);

            //Render accessories
            lock(accessoryList) {
                foreach(HairAccessory accessory in accessoryList) accessory.Render(hair);
            }
        }

        public readonly HairAccessoryData AccessoryData;
        public readonly Color AccessoryColor;
        public readonly Func<PlayerHair, bool> TargetSelector;

        private LinkedListNode<HairAccessory> accessoryListNode;
        private bool loadedTexs = false;
        private MTexture[] texs = null;

        public HairAccessory(HairAccessoryData data, Color color, Func<PlayerHair, bool> targetSelector) {
            AccessoryData = data;
            AccessoryColor = color;
            TargetSelector = targetSelector;

            //Add to accessory list
            lock(accessoryList) {
                accessoryListNode = accessoryList.AddLast(this);
            }
        }

        public void Dispose() {
            //Remove from accessory list
            lock(accessoryList) {
                if(accessoryListNode != null) accessoryList.Remove(accessoryListNode);
                accessoryListNode = null;
            }
        }

        private void Render(PlayerHair hair) {
            if(!TargetSelector(hair)) return;

            //Load textures
            if(!loadedTexs) {
                if(AccessoryData.Texture != null) texs = GFX.Game.GetAtlasSubtextures(AccessoryData.Texture).ToArray();
                loadedTexs = true;

                if(texs == null) Logger.Log(LogLevel.Warn, StylineModule.Name, $"Unknown hair accesory texture '{AccessoryData.Texture}'!");
            }
            if(texs == null) return;

            if(hair.Sprite.HasHair && hair.Sprite.HairFrame < texs.Length) {
                //Draw accessory
                texs[hair.Sprite.HairFrame].Draw(hair.Nodes[0], new Vector2(5, 5), AccessoryColor, hair.GetHairScale(0));
            }
        }
    }
}