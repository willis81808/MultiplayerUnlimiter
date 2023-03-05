using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Sons.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using Mache;
using Mache.UI;
using Il2CppInterop.Runtime.Injection;
using Sons.Gui.Multiplayer;
using Sons.Gui;
using Sons.Multiplayer.Gui;
using TheForest.Utils;

namespace MultiplayerUnlimiter
{
    [BepInDependency("com.willis.sotf.mache")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("SonsOfTheForest.exe")]
    public class MultiplayerUnlimiterPlugin : BasePlugin
    {
        public const string ModId = "com.willis.sotf.mpunlimiter";
        public const string ModName = "Multiplayer Unlimiter";
        public const string Version = "0.1.0";

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

                if (LocalPlayer._instance == null || !LocalPlayer.IsInWorld)
                {
                    var text = Mache.Mache.FindObjectOfType<CoopLobbyDialogGui>().GetComponentsInChildren<LinkTextGui>().FirstOrDefault(l => l.gameObject.name == "PlayerCount");
                    var playerCount = text.GetText().Split('/')[0];
                    text.SetText($"{playerCount}/{MaximumPlayers}");
                }

                if (PauseMenu.IsActive)
                {
                    foreach (var activePlayerList in Mache.Mache.FindObjectsOfType<ActivePlayerList>())
                    {
                        activePlayerList.SetPlayerLimit(MaximumPlayers);
                    }
                }
            }
        }

        private bool initialApplied = false;

        private void Start()
        {
            Mache.Mache.RegisterMod(() => new ModDetails
            {
                Id = MultiplayerUnlimiterPlugin.ModId,
                Version = MultiplayerUnlimiterPlugin.Version,
                Name = MultiplayerUnlimiterPlugin.ModName,
                Description = "Customize the maximum player limit!",
                OnFinishedCreating = CreateMenu,
            });
        }

        private void CreateMenu(GameObject parent)
        {
            MenuPanel.Builder()
                .AddComponent(new SliderComponent
                {
                    StartValue = MaximumPlayers,
                    MinValue = 6,
                    MaxValue = 25,
                    WholeNumbers = true,
                    Name = "Maximum Players",
                    OnValueChanged = OnMaxPlayersValueChanged
                })
                .BuildToTarget(parent);
        }
        
        private void OnMaxPlayersValueChanged(SliderComponent slider, float val)
        {
            MaximumPlayers = (int)val;
        }

        private void Update()
        {
            if (initialApplied) return;

            var lobbyManager = CoopLobbyManager.GetActiveInstance();
            if (lobbyManager == null) return;

            lobbyManager.SetMemberLimit(MaximumPlayers);
            initialApplied = true;
        }
    }
}
