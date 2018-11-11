using UnityEngine;

namespace Assets.Scripts
{
    public enum MouseEvent
    {
        Down,
        Up
    }
    
    
    public interface IBoardItemListener
    {
        void OnMouseEventReceived(object sender, MouseEvent element);
    }
    
    public class BoardItem : MonoBehaviour
    {        
        public Square CurrentSquare { get; set; }

        public IBoardItemListener MouseEventReceived { get; set; }

        protected virtual void OnMouseEventReceived(MouseEvent e)
        {
            var handler = MouseEventReceived;
            if (handler != null) handler.OnMouseEventReceived(this, e);
        }

        private void OnMouseDown()
        {
            OnMouseEventReceived(MouseEvent.Down);
        }
        
    }
}