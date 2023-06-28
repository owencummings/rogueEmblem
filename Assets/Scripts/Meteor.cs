using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Rigidbody _rb;
    private float lifespan = 3.0f;
    private float timeAlive = 0.0f;
    public TeamEnum team = TeamEnum.Enemy;
    private HashSet<int> objectsHit = new HashSet<int>();

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        _rb.mass = 10f;
    }

    void Start()
    {
        _rb.AddForce((-transform.up + transform.forward * 4) * 800f);
    }

    void FixedUpdate()
    {
        timeAlive += Time.fixedDeltaTime;
        if (timeAlive > lifespan)
        {
            Destroy(this.gameObject);
        }
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
