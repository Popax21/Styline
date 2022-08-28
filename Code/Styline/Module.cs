using System;

namespace Celeste.Mod.Styline {
    public sealed class StylineModule : EverestModule {
        public static StylineModule Instance { get; private set; }
        public static string Name => Instance.Metadata.Name;
        public StylineModule() { Instance = this; }

        public override Type SettingsType => typeof(StylineModuleSettings);
        public static StylineModuleSettings Settings => (StylineModuleSettings) Instance._Settings;
        public override Type SessionType => typeof(StylineModuleSession);
        public static StylineModuleSession Session => (StylineModuleSession) Instance._Session;

        private SettingsContentHandler settingsContentHandler = null;
        private bool settingsDirty = false;

        private PlayerProcessor playerProcessor = null;

        public override void Load() {
            //Create the player processor
            playerProcessor = new PlayerProcessor("styline-player", e => e is Player || e is BadelineDummy);
            UpdatePlayerAttributes();

            //Add hooks
            Everest.Content.OnUpdate += ContentUpdateHandler;
            On.Celeste.Level.Begin += LevelBeginHook;
            On.Celeste.Level.Update += LevelUpdateHook;
            HairAccessory.Load();
        }

        public override void Unload() {
            //Remove hooks
            Everest.Content.OnUpdate -= ContentUpdateHandler;
            On.Celeste.Level.Begin -= LevelBeginHook;
            On.Celeste.Level.Update -= LevelUpdateHook;
            HairAccessory.Unload();

            //Destroy player processor
            playerProcessor?.Dispose();
            
            //Destroy the settings content handler
            if(settingsContentHandler != null) settingsContentHandler.Dispose();
            settingsContentHandler = null;
        }

        public void UpdatePlayerAttributes() {
            if(playerProcessor != null) {
                playerProcessor.Attributes = PlayerAttributes;
                playerProcessor.Invalidate();
            }
        }

        public override void LoadContent(bool firstLoad) {
            //Create the settings content handler
            settingsContentHandler = new SettingsContentHandler(this, Settings.ContentSelections);
        }

        private void ContentUpdateHandler(ModAsset prev, ModAsset next) {
            settingsDirty = true;
        }

        private void LevelBeginHook(On.Celeste.Level.orig_Begin orig, Level level) {
            UpdatePlayerAttributes();
            orig(level);
        }

        private void LevelUpdateHook(On.Celeste.Level.orig_Update orig, Level level) {
            if(settingsDirty) {
                settingsDirty = false;
                settingsContentHandler?.Reload();
                UpdatePlayerAttributes();
            }
            orig(level);
        }

        public static IPlayerAttributes PlayerAttributes => (Session?.Enable ?? false) ? (IPlayerAttributes) Session : (IPlayerAttributes) Settings;
        public static PlayerProcessor PlayerProcessor => Instance?.playerProcessor;
    }
}