using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selectable{

    public struct Command {
        public Command(KeyCode keyPressed, Ray commandRay){
            KeyPressed = keyPressed;
            CommandRay = commandRay;
        }

        public KeyCode KeyPressed {
            get;
        }

        public Ray CommandRay {
            get;
        }
    }

    public interface ISelectable {

        Vector3 Position {
            get;
        }

        bool IsSelected {
            get;
            set;
        }

        void OnSelect();

        void OnDeselect();

        // Maybe move OnShow and OnHide somewhere else
        void OnShow();

        void OnHide();

        void OnCommand(Command command);

        public void SubscribeToSelector(){
            Selector.Instance.AddSelectable(this);
        }

        public void UnsubscribeFromSelector(){
            Selector.Instance.RemoveSelectable(this);
        }
    }
}