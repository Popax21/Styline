using Microsoft.Xna.Framework;
using Celeste.Mod.Procedurline;
using Celeste.Mod.Entities;

namespace Celeste.Mod.Styline {
    [CustomEntity("Styline/ChangePlayerAttributes")]
    public class ChangePlayerAttributes : Trigger {
        private EntityID id;
        private bool applyImmediately, applyOnce;

        private PlayerHairColorData? hairColor;
        private HairStyleData? hairStyle;
        private HairAccessoryData? hairAccessory;
        private Color? hairAccessoryColor;
        private ShirtColorData? shirtColor;
        private Color? blushColor;

        public ChangePlayerAttributes(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
            this.id = id;

            //Get flags
            applyImmediately = data.Bool("applyImmediately");
            applyOnce = data.Bool("applyOnce");

            //Preload trigger data                 
            hairColor           = LoadContent<PlayerHairColorData>(data, "hairColor");
            hairStyle           = LoadContent<HairStyleData>(data, "hairStyle");
            hairAccessory       = LoadContent<HairAccessoryData>(data, "hairAccessory");
            hairAccessoryColor  = LoadContent<Color>(data, "hairAccessoryColor");
            shirtColor          = LoadContent<ShirtColorData>(data, "shirtColor");
            blushColor          = LoadContent<Color>(data, "blushColor");
        }

        private System.Nullable<T> LoadContent<T>(EntityData data, string attrName) where T : struct {
            if(!data.Has(attrName) || data.Attr(attrName).Length <= 0) return null;
            if(Everest.Content.Get($"Content/{data.Attr(attrName)}") == null) {
                Logger.Log(LogLevel.Error, StylineModule.Name, $"Invalid value for ChangePlayerAttributes trigger attribute '{attrName}': '{data.Attr(attrName)}'");
                return null;
            } else return (System.Nullable<T>) Everest.Content.Get($"Content/{data.Attr(attrName)}").Deserialize<T>();
        }

        public override void Update() {
            base.Update();
            if(applyImmediately) {
                Apply();
                applyImmediately = false;
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            Apply();
        }

        private void Apply() {
            if(((Level) Scene).Session.GetFlag($"{id}Applied") && applyOnce) return;

            //Change session data
            if(hairColor.HasValue)          StylineModule.Session.HairColor = hairColor.Value;
            if(hairStyle.HasValue)          StylineModule.Session.HairStyle = hairStyle.Value;
            if(hairAccessory.HasValue)      StylineModule.Session.HairAccessory = hairAccessory.Value;
            if(hairAccessoryColor.HasValue) StylineModule.Session.HairAccessoryColor = hairAccessoryColor.Value;
            if(shirtColor.HasValue)         StylineModule.Session.ShirtColor = shirtColor.Value;
            if(blushColor.HasValue)         StylineModule.Session.BlushColor = blushColor.Value;
            StylineModule.Session.Enable = true;
            StylineModule.Instance.UpdatePlayerAttributes();
            
            //Set applied flag
            ((Level) Scene).Session.SetFlag($"{id}Applied");
        }
    }
}