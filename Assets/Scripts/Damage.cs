using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamEnum 
{
    Player,
    Enemy,
    Neutral
}

public interface IDamageable 
{
    int Health { get; set; }
    void OnDamage(DamageInstance damage){}
    TeamEnum Team { get; set; }
}

// Eventually add RecoilInstance class to separate damage from recoil instances
public class DamageInstance {
    public float damageValue;
    public Vector3 sourcePosition;
    // Consider adding an optional type as well
}

