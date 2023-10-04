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
        public UnitCommand(UnitCommandEnum unitCommandEnum, Vector3 targetDestination, GameObject targetGameObject, Vector3 extraPosition){
            CommandEnum = unitCommandEnum;
            TargetDestination = targetDestination;
            TargetGameObject = targetGameObject;
            ExtraPosition = extraPosition;
        }

        public UnitCommand(UnitCommandEnum unitCommandEnum, Vector3 targetDestination, GameObject targetGameObject){
            CommandEnum = unitCommandEnum;
            TargetDestination = targetDestination;
            TargetGameObject = targetGameObject;
            ExtraPosition = Vector3.zero;

        }

        public UnitCommand(UnitCommandEnum unitCommandEnum){
            CommandEnum = unitCommandEnum;
            TargetDestination = Vector3.zero;
            TargetGameObject = null;
            ExtraPosition = Vector3.zero;

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

        public Vector3 ExtraPosition {
            get;
        }
    }
    // TODO: create enum for all unit commands
    // TODO: in squad logic, translate from squad commands to unit commands
}
