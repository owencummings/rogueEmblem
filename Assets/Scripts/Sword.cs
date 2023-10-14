using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IResetable
{
    public TeamEnum team = TeamEnum.Neutral;
    private HashSet<int> objectsHit = new HashSet<int>();

    void Reset()
    {
        objectsHit = new HashSet<int>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            if (damageable.Team == team || objectsHit.Contains(damageable.ObjectID)) { return; }

            DamageInstance damage = new DamageInstance();
            damage.damageValue = 2;
            damage.sourcePosition = transform.position;
            Vector3 directionVector = damageable.SourceTransform.position - transform.position;
            Vector3 xzVector = new Vector3(directionVector.x, 0f, directionVector.z);
            damage.forceVector = xzVector.normalized * 300;
            damageable.OnDamage(damage);
            objectsHit.Add(damageable.ObjectID);
        }
    }
}
