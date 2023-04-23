using UnityEngine;

public interface IState
{
    void Tick();
    void OnEnter();
    void OnExit();
    void OnCollisionEnter(Collision collision);
}