using System;
using System.Linq;
using Microsoft.Xna.Framework;

using Monocle;
using Celeste.Mod.Procedurline;

namespace Celeste.Mod.Styline {
    public class BlushFilter : IDataProcessor<Sprite, string, SpriteAnimationData> {
        //The climb/wallslide animations are special, because they're the only animations in which the player looks sidewards
        private static readonly string[] SINGLE_BLUSH_ANIMATIONS = new string[] { "wallslide", "dangling", "climbUp", "climbLookBackStart", "climbLookBack" };

        public readonly Color BlushColor;

        public BlushFilter(Color blushColor) {
            BlushColor = blushColor.RemoveAlpha();
        }

        public void RegisterScopes(Sprite target, DataScopeKey key) {}

        public bool ProcessData(Sprite target, DataScopeKey key, string animId, ref SpriteAnimationData data) {
            if(BlushColor.A == 0) return false;
            if(data == null || !(data is PlayerSpriteAnimationData playerAnimData)) return false;

            bool didModify = false;
            for(int frameIdx = 0; frameIdx < data.Frames.Length; frameIdx++) {
                didModify |= ProcessFrame(animId, playerAnimData.PlayerFrameData[frameIdx].HairFrame, data.Frames[frameIdx].TextureData);
            }
            return didModify;
        }

        private bool ProcessFrame(string animId, int hairFrameIdx, TextureData tex) {
            //Find the face partition
            TexturePartitioning texPart = TexturePartitioning.CreateColorComponentPartitions(tex);
            texPart.MergePlayerComponents();
            int facePart = texPart.FindPartition((i, p) => PlayerTextureUtils.IsFaceComponent(texPart, i));
            if(facePart < 0) return false;

            //Calculate the blush Y coordinate
            int blushY = texPart.GetPartitionPixels(facePart).Min(p => p.Y) + 1;
            if(!texPart.GetPartitionPixels(facePart).Any(p => p.Y == blushY)) return false;

            //Find the the min/max X coordinate of face pixels on the blush Y coordinate
            int leftX  = texPart.GetPartitionPixels(facePart).Where(p => p.Y == blushY).Min(p => p.X);
            int rightX = texPart.GetPartitionPixels(facePart).Where(p => p.Y == blushY).Max(p => p.X);
            
            if(SINGLE_BLUSH_ANIMATIONS.Contains(animId, StringComparer.OrdinalIgnoreCase)) {
                //If there's only 2 pixels between left and right, we're probably in a turn-around animation
                if(rightX - leftX <= 2) {
                    //Smoothly transition blush pixel to the other side based on hair frame
                    if(hairFrameIdx != 2) return false;
                    leftX = rightX;
                }
                
                //Set only one blush pixel
                tex[(leftX + rightX) / 2, blushY] = BlushColor;
            } else {
                //If the distance between left and right isn't at least 3 pixels, it just looks weird if we set blush pixels
                if(rightX - leftX < 3) return false;

                //If the distance between left and right is exactly 3 pixels, the player is somehow hiding their left blush pixel
                if(rightX - leftX == 3) leftX = -1;

                //Set blush pixels
                if(tex.IsInBounds(leftX, blushY)) tex[leftX, blushY] = BlushColor;
                if(tex.IsInBounds(rightX, blushY)) tex[rightX, blushY] = BlushColor;
            }

            return true;
        }
    }
}