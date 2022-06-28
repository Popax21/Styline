using Microsoft.Xna.Framework;

namespace Celeste.Mod.Styline {
    public struct HairStyleData {
        public Vector2[] NodeScaleMultipliers;
        public float StepInFacingPerSegmentMultiplier, StepYSinePerSegmentMultiplier, StepApproachMultiplier;
    }

    public struct HairAccessoryData {
        public Vector2[] HairOffsets;
        public string Texture;
    }
}