using UnityEngine;

namespace GameJam.Core
{
    public interface ISelectable
    {
        bool IsSelected { get; }
        void OnSelect();
        void OnDeselect();
        void OnHover();
        void OnUnhover();
        bool IsDead();
    }
}
