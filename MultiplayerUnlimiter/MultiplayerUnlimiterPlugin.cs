using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Mache;
using Sons.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace MultiplayerUnlimiter
{
    [BepInDependency("com.willis.sotf.mache")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("SonsOfTheForest.exe")]
    public class MultiplayerUnlimiterPlugin : BasePlugin
    {
        public const string ModId = "com.willis.sotf.mpunlimiter";
        public const string ModName = "Multiplayer Unlimiter";
        public const string Version = "0.0.1";

        internal static MultiplayerUnlimiterPlugin Instance { get; private set; }

        public override void Load()
        {
            Instance = this;
            AddComponent<Unlimiter>();
        }
    }

    public class Unlimiter : MonoBehaviour
    {
        internal static Unlimiter Instance { get; private set; }

        private static UnlimiterMenu Menu { get; set; }

        private static ConfigEntry<int> _maximumPlayers;
        internal static int MaximumPlayers
        {
            get
            {
                if (_maximumPlayers == null)
                {
                    _maximumPlayers = MultiplayerUnlimiterPlugin.Instance.Config.Bind<int>(MultiplayerUnlimiterPlugin.ModId, "MaximumPlayers", 8, "The currently set maximum number of players allowed (default is 8)");
                }
                return _maximumPlayers.Value;
            }
            set
            {
                // set config value
                if (_maximumPlayers == null) return;
                _maximumPlayers.Value = value;

                // update lobby
                var lobbyManager = CoopLobbyManager.GetActiveInstance();
                if (lobbyManager == null) return;
                lobbyManager.SetMemberLimit(_maximumPlayers.Value);
            }
        }

        private bool initialApplied = false;

        private void Start()
        {
            Mache.Mache.RegisterMod(() =>
            {
                var uiBase = UniversalUI.RegisterUI(MultiplayerUnlimiterPlugin.ModId, OnMenuUpdate);
                Menu = new UnlimiterMenu(uiBase);

                return new ModDetails
                {
                    Id = MultiplayerUnlimiterPlugin.ModId,
                    Version = MultiplayerUnlimiterPlugin.Version,
                    Name = MultiplayerUnlimiterPlugin.ModName,
                    Description = "Customize the maximum player limit!",
                    OnMenuShow = ShowMenu
                };
            });
        }

        private void Update()
        {
            if (initialApplied) return;

            var lobbyManager = CoopLobbyManager.GetActiveInstance();
            if (lobbyManager == null) return;

            lobbyManager.SetMemberLimit(MaximumPlayers);
            initialApplied = true;
        }

        private void ShowMenu()
        {
            Menu.SetActive(true);
        }

        private void OnMenuUpdate() { }
    }

    public class UnlimiterMenu : UniverseLib.UI.Panels.PanelBase
    {
        public override string Name => MultiplayerUnlimiterPlugin.ModName;
        public override int MinWidth => 750;
        public override int MinHeight => 85;
        public override Vector2 DefaultAnchorMin => Vector2.zero;
        public override Vector2 DefaultAnchorMax => Vector2.zero;
        public override bool CanDragAndResize => true;

        private Text label;

        public UnlimiterMenu(UIBase owner) : base(owner) { }

        protected override void ConstructPanelContent()
        {
            var container = UIFactory.CreateHorizontalGroup(ContentRoot, "max_players_row", true, false, true, true, spacing: 10, padding: new Vector4(10, 10, 10, 10));

            label = UIFactory.CreateLabel(container, "max_players_label", $"Maximum Players ({Unlimiter.MaximumPlayers})", TextAnchor.MiddleRight);
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 85, minHeight: 30, flexibleWidth: 0);

            var sliderObj = UIFactory.CreateSlider(container, "max_players_slider", out var slider);
            UIFactory.SetLayoutElement(sliderObj, minHeight: 30, flexibleWidth: 9999);

            slider.minValue = 8;
            slider.maxValue = 20;
            slider.wholeNumbers = true;
            slider.value = Unlimiter.MaximumPlayers;

            slider.onValueChanged.AddListener(OnMaxPlayersValueChanged);

            SetActive(false);
        }

        private void OnMaxPlayersValueChanged(float val)
        {
            label.text = $"Maximum Players ({val})";
            Unlimiter.MaximumPlayers = (int)val;
        }
    }
}
