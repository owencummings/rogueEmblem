using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitBigAttack : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _transform;
    private AttackData _attackData;
    private float raiseProgress = 0f;
    private float raiseTime = 0.75f;
    private Vector3 _targetPosition;
    private bool falling = false;
    private HashSet<int> objectsHit = new HashSet<int>();

    public UnitBigAttack(NavMeshAgent navMeshAgent, Rigidbody rb, Transform transform, AttackData attackData)
    {
        _attackData = attackData;
        _navMeshAgent = navMeshAgent;
        _transform = transform;
        _rb = rb;
    }

    public void Tick()
    {
        raiseProgress += Time.deltaTime;
        if (falling){
            _attackData.attackFinished = true; 
        } else {
            if (raiseProgress > raiseTime){
                falling = true;
                _rb.velocity = Vector3.zero;
                _rb.AddForce(Vector3.down * 3000f);
            } else {
                Vector3 moveVector = (_targetPosition + 2.5f*Vector3.up - _transform.position);
                _rb.velocity = moveVector.normalized * 5f;
            }
        }

    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _transform.LookAt(_attackData.attackTarget.transform);
        _rb.isKinematic = false;
        _rb.AddForce(_transform.up * 300 + _transform.forward * 100);
        _attackData.attackFinished = false;
        _targetPosition = _attackData.attackTarget.transform.position;
        falling = false;
        raiseProgress = 0;
        objectsHit = new HashSet<int>();
    }

    public void OnExit()
    {
        _attackData.attackTarget = null;
        _attackData.attackFinished = false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            if (!falling || damageable.Team ==  _attackData.team || objectsHit.Contains(damageable.ObjectID)) { return; }

            DamageInstance damage = new DamageInstance();
            damage.damageValue = 2;
            damage.sourcePosition = _transform.position;
            Vector3 directionVector = damageable.SourceTransform.position - _transform.position;
            Vector3 xzVector = new Vector3(directionVector.x, 0f, directionVector.z);
            damage.forceVector = xzVector.normalized * 300;
            damageable.OnDamage(damage);
            objectsHit.Add(damageable.ObjectID);
        }
    }
}
