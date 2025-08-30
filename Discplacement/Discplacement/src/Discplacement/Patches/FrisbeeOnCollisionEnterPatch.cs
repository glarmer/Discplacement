using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace Discplacement.Patches;

[HarmonyPatch(typeof(Frisbee), nameof(Frisbee.OnCollisionEnter))]
public class FrisbeeOnCollisionEnterPatch
{
    static void Prefix(Frisbee __instance, out bool __state)
    {
        __state = __instance.throwValidForAchievement;
    }
    
    
    static void Postfix(Frisbee __instance, Collision collision, bool __state)
    {
        if (Plugin.ConfigurationHandler.IsUsesCapped && __instance.item.GetData<FloatItemData>(DataEntryKey.UseRemainingPercentage).Value == 0) return; //item use option
        
        DiscplacementComponent discplacementComponent = __instance.gameObject.GetComponent<DiscplacementComponent>();
        
        if (discplacementComponent.IsOnCooldown(__instance.item.data.guid)) return;
        
        if (discplacementComponent.HasTeleported && __instance.item.itemState == ItemState.Ground) return;

        if (discplacementComponent.HasTeleported && __instance.item.itemState == ItemState.Held)
        {
            discplacementComponent.HasTeleported = false;
            return;
        }
        if (__instance.item.itemState == ItemState.Held) return;
        
        Plugin.Logger.LogInfo("Frisbee has collided with:");
        Plugin.Logger.LogInfo(collision.transform.gameObject.name);

        Character character = __instance.item.lastThrownCharacter;

        if (__instance.item.lastThrownAmount == 0) return;
        
        Plugin.Logger.LogInfo("Frisbee last character: " + character.name);

        Vector3 position = __instance.transform.position;
        Vector3 contactPoint = collision.contacts[0].point;
        
        Vector3 direction = (position - contactPoint).normalized;
        
        character.view.RPC("WarpPlayerRPC", RpcTarget.All, position + direction*0.7f, true);

        if (Plugin.ConfigurationHandler.IsBalanceCooldownEnabled)
        {
            Plugin.Logger.LogInfo("Trying to activate cooldown");
            discplacementComponent.ActivateCooldown(__instance.item);
            Plugin.Logger.LogInfo("Cooldown time remaining " + discplacementComponent.CooldownTimeRemaining(__instance.item.data.guid));
        }
        
        if (Plugin.ConfigurationHandler.IsUsesCapped)
        {
            Plugin.Logger.LogInfo("Trying to reduce uses");
            discplacementComponent.reduceUses.RunAction();
            Plugin.Logger.LogInfo("Trying to reduce uses 2");
        }
        
        discplacementComponent.HasTeleported = true;

        if (Plugin.ConfigurationHandler.IsFrisbeeReturnEnabled)
        {
            __instance.item.Interact(character);
        }
    }
}