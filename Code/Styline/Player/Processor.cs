using System;

using Monocle;
using Celeste.Mod.Procedurline;

namespace Celeste.Mod.Styline {
    public sealed class PlayerProcessor : DataScope, IDataProcessor<Sprite, string, SpriteAnimationData>, IDataProcessor<Player, VoidBox, PlayerHairColorData>, IDataProcessor<Player, VoidBox, PlayerHairSettingsData>, IDataProcessor<PlayerHair, int, PlayerHairNodeData> {
        private readonly object LOCK = new object();
        public readonly Func<Entity, bool> TargetSelector;
        public readonly bool ScaleHairNodes;

        private readonly CompositeAsyncDataProcessor<Sprite, string, SpriteAnimationData>.ProcessorHandle animProcHandle;
        private readonly CompositeDataProcessor<Player, VoidBox, PlayerHairColorData>.ProcessorHandle hairColorProcHandle;
        private readonly CompositeDataProcessor<Player, VoidBox, PlayerHairSettingsData>.ProcessorHandle hairSettingsProcHandle;
        private readonly CompositeDataProcessor<PlayerHair, int, PlayerHairNodeData>.ProcessorHandle hairNodeProcHandle;

        private IPlayerAttributes attributes;
        private ShirtColorFilter shirtColorFilter;
        private BlushFilter blushFilter;
        private HairAccessory hairAccessory;

        public PlayerProcessor(string name, Func<Entity, bool> targetSelector, bool scaleHairNodes = true) : base(name) {
            TargetSelector = targetSelector;
            ScaleHairNodes = scaleHairNodes;

            //Add processors
            animProcHandle = ProcedurlineModule.SpriteManager.AnimationProcessor.AddProcessor(0, this.WrapAsync<Sprite, string, SpriteAnimationData>());
            hairColorProcHandle = ProcedurlineModule.PlayerManager.HairColorProcessor.AddProcessor(0, this);
            hairSettingsProcHandle = ProcedurlineModule.PlayerManager.HairSettingsProcessor.AddProcessor(0, this);
            hairNodeProcHandle = ProcedurlineModule.PlayerManager.HairNodeProcessor.AddProcessor(0, this);

            //Re-assign all player scopes
            ProcedurlineModule.PlayerScope?.InvalidateRegistrars();
        }

        public override void Dispose() {
            //Remove processors
            animProcHandle?.Dispose();
            hairColorProcHandle?.Dispose();
            hairSettingsProcHandle?.Dispose();
            hairNodeProcHandle?.Dispose();

            //Dispose hair accessory
            hairAccessory?.Dispose();

            base.Dispose();
        }

        public void RegisterScopes(Sprite target, DataScopeKey key) {
            lock(LOCK) {
                if(!(target is PlayerSprite) || !TargetSelector(target.Entity)) return;
                RegisterKey(key);
                shirtColorFilter?.RegisterScopes(target, key);
                blushFilter?.RegisterScopes(target, key);
            }
        }

        public void RegisterScopes(Player target, DataScopeKey key) {
            lock(LOCK) {
                if(!TargetSelector(target)) return;
                RegisterKey(key);
            }
        }

        public void RegisterScopes(PlayerHair target, DataScopeKey key) {
            lock(LOCK) {
                if(!TargetSelector(target.Entity)) return;
                RegisterKey(key);
            }
        }

        public bool ProcessData(Sprite target, DataScopeKey key, string id, ref SpriteAnimationData data)  {
            lock(LOCK) {
                if(!(attributes?.Enable ?? false) || !(target is PlayerSprite) || !TargetSelector(target.Entity)) return false;
                bool didModify = false;
                didModify |= blushFilter?.ProcessData(target, key, id, ref data) ?? false;
                didModify |= shirtColorFilter?.ProcessData(target, key, id, ref data) ?? false;
                return didModify;
            }
        }

        public bool ProcessData(Player target, DataScopeKey key, VoidBox id, ref PlayerHairColorData data) {
            lock(LOCK) {
                if(!(attributes?.Enable ?? false) || !TargetSelector(target)) return false;
                //FIXME Badeline hair colors
                data.UsedColor = attributes.HairColor.UsedColor.RemoveAlpha();
                data.NormalColor = attributes.HairColor.NormalColor.RemoveAlpha();
                data.TwoDashesColor = attributes.HairColor.TwoDashesColor.RemoveAlpha();
                return true;
            }
        }

        public bool ProcessData(Player target, DataScopeKey key, VoidBox id, ref PlayerHairSettingsData data) {
            lock(LOCK) {
                if(!(attributes?.Enable ?? false) || !TargetSelector(target)) return false;
                data.NodeCount = attributes.HairStyle.NodeScaleMultipliers.Length;
                data.StepInFacingPerSegment *= attributes.HairStyle.StepInFacingPerSegmentMultiplier;
                data.StepYSinePerSegment *= attributes.HairStyle.StepYSinePerSegmentMultiplier;
                data.StepApproach *= attributes.HairStyle.StepApproachMultiplier;
                return true;
            }
        }

        public bool ProcessData(PlayerHair target, DataScopeKey key, int idx, ref PlayerHairNodeData data) {
            lock(LOCK) {
                if(!(attributes?.Enable ?? false) || !TargetSelector(target.Entity)) return false;
                if(ScaleHairNodes && idx < attributes.HairStyle.NodeScaleMultipliers.Length) data.Scale *= attributes.HairStyle.NodeScaleMultipliers[idx];
                return true;
            }
        }

        public IPlayerAttributes Attributes {
            get => attributes;
            set {
                lock(LOCK) {
                    //Dispose old hair accessory
                    hairAccessory?.Dispose();

                    attributes = value;

                    if(attributes?.Enable ?? false) {
                        //Create filters
                        shirtColorFilter = new ShirtColorFilter(attributes.ShirtColor);
                        blushFilter = new BlushFilter(attributes.BlushColor);

                        //Create hair accessory
                        hairAccessory = new HairAccessory(attributes.HairAccessory, attributes.HairAccessoryColor, h => TargetSelector(h.Entity));
                    }
                }

                Invalidate();
            }
        }
    }
}