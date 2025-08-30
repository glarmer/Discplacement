using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Discplacement.Patches;

public class FrisbeeOnEnablePatch
{
    [HarmonyPatch(typeof(Frisbee), "OnEnable")]
    [HarmonyPostfix]
    static void Postfix(Frisbee __instance)
    {
        DiscplacementComponent dc = __instance.gameObject.AddComponent<DiscplacementComponent>();
        dc.Init(__instance.item);
        
        ItemParticles particles = __instance.gameObject.GetComponent<ItemParticles>();
        
        if (DiscplacementComponent.fireParticles != null && particles != null)
        {
            ParticleSystem cloned = Object.Instantiate(
                DiscplacementComponent.fireParticles,
                __instance.transform
            );
            
            particles.smoke = cloned;
            particles.smoke.Clear();
            particles.smoke.Play();
        }
    }
}