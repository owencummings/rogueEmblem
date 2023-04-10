using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selectable{
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

        public void SubscribeToSelector(){
            Selector.Instance.AddSelectable(this);
        }

        public void UnsubscribeFromSelector(){
            Selector.Instance.RemoveSelectable(this);
        }
    }
}