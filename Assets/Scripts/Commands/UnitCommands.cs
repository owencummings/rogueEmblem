using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitCommands
{
    public enum UnitCommandEnum {
        None,
        Rally,
        Attack,
        Carry,
        Jump,
        Cancel
    }

    public struct UnitCommand {
        public UnitCommand(UnitCommandEnum unitCommandEnum, Vector3 targetDestination, GameObject targetGameObject){
            CommandEnum = unitCommandEnum;
            TargetDestination = targetDestination;
            TargetGameObject = targetGameObject;
        }

        public UnitCommand(UnitCommandEnum unitCommandEnum){
            CommandEnum = unitCommandEnum;
            TargetDestination = Vector3.zero;
            TargetGameObject = null;
        }

        public UnitCommandEnum CommandEnum {
            get;
        }

        public Vector3 TargetDestination {
            get;
        }

        public GameObject TargetGameObject {
            get;
        }
    }
    // TODO: create enum for all unit commands
    // TODO: in squad logic, translate from squad commands to unit commands
}
