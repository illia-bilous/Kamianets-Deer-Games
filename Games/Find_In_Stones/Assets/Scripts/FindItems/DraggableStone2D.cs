using UnityEngine;

namespace FindInStones
{
    [RequireComponent(typeof(Collider2D))]
    public class DraggableStone2D : Draggable2D
    {
        protected override void OnDragEnded(Vector2 screenPoint)
        {
            RememberStartTransform();
        }
    }
}
