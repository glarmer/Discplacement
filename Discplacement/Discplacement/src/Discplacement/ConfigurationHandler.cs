using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine.InputSystem;

namespace Discplacement;

public class ConfigurationHandler
{
    private ConfigFile _config = new ConfigFile(Path.Combine(Paths.ConfigPath, "Discplacement.cfg"), true);
    public InputAction MenuAction { get; set; }
    
    public ConfigEntry<int> ConfigBalanceUses;
    public ConfigEntry<bool> ConfigBalanceUsesCapped;
    
    public ConfigEntry<bool> ConfigFrisbeeReturn;
    
    public ConfigEntry<int> ConfigBalanceCooldown;
    public ConfigEntry<bool> ConfigBalanceCooldownEnabled;
    public ConfigEntry<string> ConfigMenuKey;
    public ConfigEntry<bool> ConfigFrisbeeParticleEffectEnabled;
    public int Uses => ConfigBalanceUses.Value;
    public bool IsUsesCapped => ConfigBalanceUsesCapped.Value;
    
    public bool IsFrisbeeReturnEnabled => ConfigFrisbeeReturn.Value;
    public bool IsBalanceCooldownEnabled => ConfigBalanceCooldownEnabled.Value;
    public int Cooldown => ConfigBalanceCooldown.Value;
    public bool IsFrisbeeParticleEffectEnabled => ConfigFrisbeeParticleEffectEnabled.Value;
    
    
    public ConfigurationHandler()
    {
        Plugin.Logger.LogInfo("ConfigurationHandler initialising");
        
        ConfigBalanceUsesCapped = _config.Bind
        (
            "Balance: Uses",
            "Cap Uses",
            false,
            "Whether or not the frisbee uses are capped. (false = infinite uses)"
        );
        
        ConfigBalanceUsesCapped.SettingChanged += OnBalanceUsesCappedChanged;
        Plugin.Logger.LogInfo("ConfigurationHandler: Uses Cap Enabled: " + ConfigBalanceUsesCapped.Value);
        
        ConfigBalanceUses = _config.Bind
        (
            "Balance: Uses",
            "Uses",
            3,
            "The number of times the frisbee can teleport you."
        );
        if (ConfigBalanceUses.Value <= 0)
        {
            ConfigBalanceUses.Value = 1;
        }
        ConfigBalanceUses.SettingChanged += OnBalanceUsesChanged;
        Plugin.Logger.LogInfo("ConfigurationHandler: Uses Loaded: " + ConfigBalanceUses.Value);
        
        ConfigFrisbeeReturn = _config.Bind
        (
            "Gameplay",
            "Frisbee Return",
            true,
            "Whether or not the frisbee should be returned to your inventory automatically."
        );
        
        ConfigFrisbeeReturn.SettingChanged += OnFrisbeeReturnChanged;
        Plugin.Logger.LogInfo("ConfigurationHandler: Uses Cap Enabled: " + ConfigFrisbeeReturn.Value);
        
        
        ConfigBalanceCooldownEnabled = _config.Bind
        (
            "Balance: Cooldown",
            "Cooldown Enabled",
            true,
            "Whether or not the frisbee cooldown is enabled."
        );
        ConfigBalanceCooldownEnabled.SettingChanged += OnBalanceCooldownEnabledChanged;
        Plugin.Logger.LogInfo($"Set Cooldown Enabled to " + IsBalanceCooldownEnabled + "!");
        
        ConfigFrisbeeParticleEffectEnabled = _config.Bind
        (
            "General",
            "Frisbee Fire Particles Enabled",
            true,
            "Whether or not you want the fire particle effects on the frisbee."
        );
        ConfigFrisbeeParticleEffectEnabled.SettingChanged += OnFrisbeeParticleEffectEnabledChanged;
        Plugin.Logger.LogInfo($"Set Cooldown Enabled to " + IsFrisbeeParticleEffectEnabled + "!");
        
        ConfigBalanceCooldown = _config.Bind
        (
            "Balance: Cooldown",
            "Cooldown Time",
            8,
            "How long in seconds the cooldown should be."
        );
        ConfigBalanceCooldown.SettingChanged += OnBalanceCooldownChanged;
        Plugin.Logger.LogInfo($"Set the Cooldown to " + Cooldown + "!");
        
        ConfigMenuKey = _config.Bind
        (
            "General",
            "Config Menu Key",
            "<Keyboard>/f3",
            "Control path for opening the mod configuration menu (e.g. <Keyboard>/f3, <Keyboard>/space, <Keyboard>/escape)"
        );
        Plugin.Logger.LogInfo("ConfigurationHandler: Config Menu Key: " + ConfigMenuKey.Value);
        SetupInputAction();
        ConfigMenuKey.SettingChanged += OnMenuKeyChanged;
        
        Plugin.Logger.LogInfo("ConfigurationHandler initialised");
    }
    
    private void OnBalanceCooldownChanged(object sender, System.EventArgs e)
    {
        DiscplacementComponent.CooldownDuration = Cooldown;
        Plugin.Logger.LogInfo($"Set the Cooldown to " + Cooldown + "!");
    }
    
    private void OnFrisbeeParticleEffectEnabledChanged(object sender, System.EventArgs e)
    {
        Plugin.Logger.LogInfo($"Set the Particles Enabled to " + IsFrisbeeParticleEffectEnabled + "!");
    }
    
    private void OnBalanceCooldownEnabledChanged(object sender, System.EventArgs e)
    {
        if (ConfigBalanceCooldownEnabled.Value)
        {
            DiscplacementComponent._totalUses = 100;
            ConfigBalanceUsesCapped.Value = false;
        }
        Plugin.Logger.LogInfo($"Set Cooldown Enabled to " + IsBalanceCooldownEnabled + "!");
    }
    private void OnBalanceUsesChanged(object sender, System.EventArgs e)
    {
        if (IsUsesCapped)
        {
            DiscplacementComponent._totalUses = Uses;
        }
        Plugin.Logger.LogInfo($"Set the Uses to " + Uses + "!");
    }
    
    private void OnBalanceUsesCappedChanged(object sender, System.EventArgs e)
    {
        if (ConfigBalanceUsesCapped.Value)
        {
            DiscplacementComponent._totalUses = Uses;
            ConfigBalanceCooldownEnabled.Value = false;
        }
        Plugin.Logger.LogInfo($"Set the Uses Cap Enabled to " + IsUsesCapped + "!");
    }
    
    private void OnFrisbeeReturnChanged(object sender, System.EventArgs e)
    {
        Plugin.Logger.LogInfo($"Set the Uses Cap Enabled to " + IsFrisbeeReturnEnabled + "!");
    }
    
    private void OnMenuKeyChanged(object sender, System.EventArgs e)
    {
        SetupInputAction();
    }
    
    private void SetupInputAction()
    {
        MenuAction?.Dispose();

        MenuAction = new InputAction(type: InputActionType.Button);
        MenuAction.AddBinding(ConfigMenuKey.Value);
        MenuAction.Enable();
    }
}