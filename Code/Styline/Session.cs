using Microsoft.Xna.Framework;

using Celeste.Mod.Procedurline;

namespace Celeste.Mod.Styline {
    public class StylineModuleSession : EverestModuleSession, IPlayerAttributes {
        public bool Enable { get; set; }
        public PlayerHairColorData HairColor { get; set; }
        public HairStyleData HairStyle { get; set; }
        public HairAccessoryData HairAccessory { get; set; }
        public Color HairAccessoryColor { get; set; }
        public ShirtColorData ShirtColor { get; set; }
        public Color BlushColor { get; set; }
    }
}