using Microsoft.Xna.Framework;
using Celeste.Mod.Procedurline;
using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.DataTypes;

namespace Celeste.Mod.Styline {
    public class DataPlayerAttributes : DataType<DataPlayerAttributes>, IPlayerAttributes {
        static DataPlayerAttributes() => DataID = $"stylinePlayerAttributesV{CelesteNetSyncComponent.CelesteNetDataVersion}";

        public DataPlayerInfo Player;

        public bool Enable { get; set; }

        public PlayerHairColorData HairColor { get; set; }
        public HairStyleData HairStyle { get; set; }
        public HairAccessoryData HairAccessory { get; set; }
        public Color HairAccessoryColor { get; set; }

        public ShirtColorData ShirtColor { get; set; }
        public Color BlushColor { get; set; }

        public DataPlayerAttributes() {}
        public DataPlayerAttributes(DataPlayerInfo player, IPlayerAttributes attrs) {
            Player = player;

            Enable = attrs.Enable;

            HairColor = attrs.HairColor;
            HairStyle = attrs.HairStyle;
            HairAccessory = attrs.HairAccessory;
            HairAccessoryColor = attrs.HairAccessoryColor;

            ShirtColor = attrs.ShirtColor;
            BlushColor = attrs.BlushColor;
        }

        public override MetaType[] GenerateMeta(DataContext ctx) => new MetaType[] { new MetaPlayerPrivateState(Player), new MetaBoundRef(DataType<DataPlayerInfo>.DataID, Player?.ID ?? uint.MaxValue, true) };

        public override void FixupMeta(DataContext ctx) {
            Player = Get<MetaPlayerPrivateState>(ctx);
            Get<MetaBoundRef>(ctx).ID = Player?.ID ?? uint.MaxValue;
        }

        protected override void Read(CelesteNetBinaryReader reader) {
            Enable = reader.ReadBoolean();
            if(Enable) {
                //Read hair data
                PlayerHairColorData hairCol = default;
                hairCol.NormalColor = reader.ReadColorNoA();
                hairCol.TwoDashesColor = reader.ReadColorNoA();
                hairCol.UsedColor = reader.ReadColorNoA();
                HairColor = hairCol;

                HairStyleData hairStyle = default;
                hairStyle.NodeScaleMultipliers = new Vector2[reader.ReadByte()];
                for(int i = 0; i < hairStyle.NodeScaleMultipliers.Length; i++) hairStyle.NodeScaleMultipliers[i] = reader.ReadVector2Scale();
                hairStyle.StepInFacingPerSegmentMultiplier = reader.ReadSingle();
                hairStyle.StepYSinePerSegmentMultiplier = reader.ReadSingle();
                hairStyle.StepApproachMultiplier = reader.ReadSingle();
                HairStyle = hairStyle;

                HairAccessoryData hairAccessory = default;
                hairAccessory.HairOffset = reader.ReadVector2();
                hairAccessory.Texture = reader.ReadNetString();
                HairAccessory = hairAccessory;
                HairAccessoryColor = reader.ReadColor();

                //Read other data
                ShirtColorData shirtColor = default;
                shirtColor.PrimaryColor = reader.ReadColorNoA();
                shirtColor.SecondaryColor = reader.ReadColorNoA();
                ShirtColor = shirtColor;

                BlushColor = reader.ReadColor();
            }
        }

        protected override void Write(CelesteNetBinaryWriter writer) {
            writer.Write(Enable);
            if(Enable) {
                //Write hair data
                writer.WriteNoA(HairColor.NormalColor);
                writer.WriteNoA(HairColor.TwoDashesColor);
                writer.WriteNoA(HairColor.UsedColor);

                writer.Write((byte) HairStyle.NodeScaleMultipliers.Length);
                foreach(Vector2 v in HairStyle.NodeScaleMultipliers) writer.Write(v);
                writer.Write(HairStyle.StepInFacingPerSegmentMultiplier);
                writer.Write(HairStyle.StepYSinePerSegmentMultiplier);
                writer.Write(HairStyle.StepApproachMultiplier);

                writer.Write(HairAccessory.HairOffset);
                writer.WriteNetString(HairAccessory.Texture);
                writer.Write(HairAccessoryColor);

                //Write other data
                writer.WriteNoA(ShirtColor.PrimaryColor);
                writer.WriteNoA(ShirtColor.SecondaryColor);

                writer.Write(BlushColor);
            }
        }
    }
}