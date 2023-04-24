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
    TeamEnum Team { get; set; }
    void OnDamage(DamageInstance damage){}
}

// The most common implementation of IDamageable
public interface IStandardDamageable : IDamageable
{
    new int Health { get; set; }
    new TeamEnum Team { get; set; }
    Queue<DamageInstance> DamageQueue { get; set; }
    new void OnDamage(DamageInstance damage){
        DamageQueue.Enqueue(damage);
    }
}

// Eventually add RecoilInstance class to separate damage from recoil instances
public class DamageInstance {
    public float damageValue;
    public Vector3 sourcePosition;
    // Consider adding an optional type as well
}

