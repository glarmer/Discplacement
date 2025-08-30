using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Discplacement.Patches;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Discplacement;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }
    public static ConfigurationHandler ConfigurationHandler { get; private set; }
    private readonly Harmony _harmony = new(Id);
    private ModConfigurationUI _ui;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {Name} is loaded!");
        ConfigurationHandler = new ConfigurationHandler();
        
        _harmony.PatchAll(typeof(FrisbeeOnCollisionEnterPatch));
        Logger.LogInfo("OnCollisionEnter Patch Loaded!");
        _harmony.PatchAll(typeof(FrisbeeOnEnablePatch));
        Logger.LogInfo("OnEnablePatch Patch Loaded!");
        
        //Mod Configuration Menu
        var go = new GameObject("PEAKUnlimitedUI");
        DontDestroyOnLoad(go);
        _ui = go.AddComponent<ModConfigurationUI>();
        _ui.Init(new List<Option>
        {
            Option.Bool("Cooldown Enabled", ConfigurationHandler.ConfigBalanceCooldownEnabled),
            Option.Int("Cooldown", ConfigurationHandler.ConfigBalanceCooldown, 1, 30),
            Option.Bool("Uses Enabled", ConfigurationHandler.ConfigBalanceUsesCapped),
            Option.Int("Uses", ConfigurationHandler.ConfigBalanceUses, 1, 30),
            Option.Bool("Frisbee Return", ConfigurationHandler.ConfigFrisbeeReturn),
            Option.Bool("Frisbee Particles", ConfigurationHandler.ConfigFrisbeeParticleEffectEnabled),
            Option.InputAction("Menu Key", ConfigurationHandler.ConfigMenuKey)
        });
    }
}