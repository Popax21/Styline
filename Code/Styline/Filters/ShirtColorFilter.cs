using System.Linq;
using Microsoft.Xna.Framework;

using Monocle;
using Celeste.Mod.Procedurline;

namespace Celeste.Mod.Styline {
    public struct ShirtColorData {
        public Color PrimaryColor;
        public Color SecondaryColor;

        public ShirtColorData RemoveAlpha() => new ShirtColorData() {
            PrimaryColor = PrimaryColor.RemoveAlpha(),
            SecondaryColor = SecondaryColor.RemoveAlpha()
        };
    };

    public class ShirtColorFilter : IDataProcessor<Sprite, string, SpriteAnimationData>, IDataProcessor<Sprite, int, SpriteAnimationData.AnimationFrame>  {
        public readonly ShirtColorData ColorData;

        public ShirtColorFilter(ShirtColorData colorData) {
            ColorData = colorData.RemoveAlpha();
        }

        public void RegisterScopes(Sprite target, DataScopeKey key) {}

        public bool ProcessData(Sprite target, DataScopeKey key, string id, ref SpriteAnimationData data) {
            if(data == null) return false;
            return data.ApplyFrameProcessor(this, target, key);
        }

        public bool ProcessData(Sprite target, DataScopeKey key, int id, ref SpriteAnimationData.AnimationFrame data) {
            //Iterate over all pixels in the shirt
            for(int x = 0; x < data.Width; x++) {
                for(int y = 0; y < data.Height; y++) {
                    //Replace color
                    if(data.TextureData[x,y] == PlayerTextureUtils.SHIRT_PRIMARY_COLOR) data.TextureData[x,y] = ColorData.PrimaryColor;
                    else if(data.TextureData[x,y] == PlayerTextureUtils.SHIRT_SECONDARY_COLOR) data.TextureData[x,y] = ColorData.SecondaryColor;
                }
            }

            return true;
        }
    }
}