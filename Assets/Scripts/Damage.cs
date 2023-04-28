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
    float Health { get; set; }
    TeamEnum Team { get; set; }
    Transform SourceTransform { get; set; }
    int ObjectID { get; set; }
    void OnDamage(DamageInstance damage){}
}

// The most common implementation of IDamageable, WIP
public interface IStandardDamageable : IDamageable
{
    new float Health { get; set; }
    new TeamEnum Team { get; set; }
    new Transform SourceTransform { get; }
    Queue<DamageInstance> DamageQueue { get; set; }
    new void OnDamage(DamageInstance damage){
        DamageQueue.Enqueue(damage);
    }
}

// Eventually add RecoilInstance class to separate damage from recoil instances
public class DamageInstance {
    public float damageValue;
    public Vector3 sourcePosition; // Maybe not necessary
    public Vector3 forceVector;
    // Consider adding an optional type as well
}

