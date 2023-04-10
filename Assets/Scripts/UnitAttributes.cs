using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitAttributes
{
    public interface IActiveOnTurn{
        void OnTurn();
    }

    public interface IDamageable{
        int Health { get; set; }
        void Damage(int damage){
            Health -= damage;
        }
    }

    public interface IOnGrid{
        Vector2 Location { get; set; }
    }
}
