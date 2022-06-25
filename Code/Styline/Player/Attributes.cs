using Microsoft.Xna.Framework;

using Celeste.Mod.Procedurline;

namespace Celeste.Mod.Styline {
    public interface IPlayerAttributes {
        bool Enable { get; }

        PlayerHairColorData HairColor { get; }
        HairStyleData HairStyle { get; }
        HairAccessoryData HairAccessory { get; }
        Color HairAccessoryColor { get; }

        ShirtColorData ShirtColor { get; }
        Color BlushColor { get; }
    }
}