using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Discplacement;

public class DiscplacementComponent : MonoBehaviour
{
    public bool HasTeleported { get; set; } = false;
    public Action_ReduceUses reduceUses;
    public static float CooldownDuration { get; set; } = 8f;
    private static Dictionary<Guid, float> _cooldowns = new Dictionary<Guid, float>();
    public static int _totalUses { get; set; } = 100 ;
    public static ParticleSystem fireParticles { get; set; }
    private Item item;
    ItemParticles particles;

    public void Awake()
    {
        GameObject lanternPrefab = Resources.Load<GameObject>("0_Items/Lantern");
        GameObject lanternInstance = Instantiate(
            lanternPrefab,
            new Vector3(0, 0, 0),
            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
        );
        
        Item lanternItem = lanternInstance.GetComponent<Item>();

        Lantern lantern = lanternItem.GetComponent<Lantern>();
        
        Transform transform = lantern.transform;
        transform.position = lantern.transform.position + new Vector3(0f, -5000f, 0f);
        if (lantern != null && lantern.fireParticle != null)
        {
            ParticleSystem cloned = Instantiate(
                lantern.fireParticle,
                transform
            );
            cloned.transform.localPosition = Vector3.zero;
            cloned.transform.localScale = Vector3.one;
            
            var main = cloned.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.3f, 1f, 1f));
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.simulationSpeed = 0.2f;
            
            var colorOverLifetime = cloned.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 0.4f, 1f), 0.0f),
                    new GradientColorKey(new Color(0.6f, 0.0f, 0.8f), 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.3f, 0.0f),
                    new GradientAlphaKey(0f, 1.0f)
                }
            );
            colorOverLifetime.color = gradient;
            
            var renderer = cloned.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = new Color(1f, 0.4f, 1f, 1f); 
            }
            var shape = cloned.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.08f;
            shape.rotation = new Vector3(0f, 0f, 0f);
            shape.position = new Vector3(0f, 0.01f, 0f);

            var velocityOverLifetime = cloned.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);

            DiscplacementComponent.fireParticles = cloned;
        }
        if (lanternItem != null)
            Object.Destroy(lanternItem.gameObject);
        
        particles = gameObject.GetComponent<ItemParticles>();
    }
    
    public void Init(Item item)
    {
        reduceUses = this.gameObject.AddComponent<Action_ReduceUses>();
        reduceUses.item = item;
        reduceUses.consumeOnFullyUsed = false;
        if (Plugin.ConfigurationHandler.IsUsesCapped)
        {
            _totalUses = Plugin.ConfigurationHandler.Uses;
        }

        this.item = item;
        this.item.totalUses = _totalUses;
        CooldownDuration = Plugin.ConfigurationHandler.Cooldown;
    }

    public void UpdateUses()
    {
        gameObject.GetComponent<Item>().totalUses = _totalUses;
    }
    public void Update()
    {
        float percentageStored = item.GetData<FloatItemData>(DataEntryKey.UseRemainingPercentage).Value;
        Plugin.Logger.LogInfo("PERCENTAGE " + percentageStored);
        if (Plugin.ConfigurationHandler.IsUsesCapped)
        {
            if (!particles.smoke.isPlaying && percentageStored > 0)
            {
                particles.smoke.Play();
            }
            else if (particles.smoke.isPlaying && percentageStored == 0)
            {
                particles.smoke.Stop();
            }
        }
        item.totalUses = _totalUses;
        if (Plugin.ConfigurationHandler.IsBalanceCooldownEnabled)
        {
            Guid guid = item.data.guid;
            if (IsOnCooldown(guid))
            {
                float percentage = 1f - CooldownTimeRemaining(guid) / CooldownDuration;
                item.SetUseRemainingPercentage(percentage);
            } else if (percentageStored < 1)
            {
                item.SetUseRemainingPercentage(1);
            }
            if (!particles.smoke.isPlaying && percentageStored > 0.9)
            {
                particles.smoke.Play();
            }
            else if (particles.smoke.isPlaying && percentageStored < 0.9)
            {
                particles.smoke.Stop();
            }
        }

        if (!Plugin.ConfigurationHandler.IsFrisbeeParticleEffectEnabled)
        {
            particles.smoke.Stop();
        }
    }

    public void ActivateCooldown(Item item)
    {
        float cooldownEndTime = Time.time + CooldownDuration;
        if (!_cooldowns.TryAdd(item.data.guid, cooldownEndTime))
        {
            _cooldowns[item.data.guid] = cooldownEndTime;
        }
        item.SetUseRemainingPercentage(0);
        Plugin.Logger.LogInfo("Cooldown started at " + Time.time);
    }

    public bool IsOnCooldown(Guid guid)
    {
        if (_cooldowns.TryGetValue(guid, out var cooldown))
        {
            return Time.time < cooldown;
        }
        return false;
    }

    public float CooldownTimeRemaining(Guid guid)
    {
        if (_cooldowns.TryGetValue(guid, out var cooldown))
        {
            return Mathf.Max(0, cooldown - Time.time);
        }
        return 0;
    }
}