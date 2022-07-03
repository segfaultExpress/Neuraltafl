using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unitafl
{
    public class PieceBehavior : MonoBehaviour
    {
        public BoardController controller;
        
        private Vector3 originalPos;

        private Vector3 mOffset;
        private float mZCoord;

        public Animator anim;

        /// <summary>
        /// Loads a piece, animations, and interactions
        /// </summary>
        void Start()
        {
            Debug.Log("Creating script for new PieceU");
            // No idea if this is ideal, personally would rather do the JS-style that.setRef(this);
            anim = Resources.Load("PieceAnimator", typeof(Animator)) as Animator;
            controller = GameObject.Find("GameController").GetComponent<BoardController>();
        }
        
        /// <summary>
        /// On piece "click" or - in XR - "modification start"
        /// </summary>
        public void OnMouseDown()
        {
            Debug.Log("Piece Clicked!");
            originalPos = gameObject.transform.position;

            mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

            // Store offset = gameobject world pos - mouse world pos
            mOffset = gameObject.transform.position - GetMouseAsWorldPoint();

            controller.selectPiece(this.gameObject);
        }

        /// <summary>
        /// On drag and update via old input mechanics, get where the mouse is "pointing" in 3d space
        /// </summary>
        /// <returns>A Vector3 that is the world poitn</returns>
        private Vector3 GetMouseAsWorldPoint()
        {
            // Pixel coordinates of mouse (x,y)
            Vector3 mousePoint = Input.mousePosition;

            // z coordinate of game object on screen
            mousePoint.z = mZCoord;

            // Convert it to world points
            return Camera.main.ScreenToWorldPoint(mousePoint);

        }

        /// <summary>
        /// On drag of a mouse, update the position of the piece
        /// </summary>
        void OnMouseDrag()
        {
            transform.position = GetMouseAsWorldPoint() + mOffset;
        }

        /// <summary>
        /// On piece "un click" or - in XR - "modification end"
        /// </summary>
        public void OnMouseUp()
        {
            Debug.Log("Piece Released!");

            if (!controller.move())
            {
                transform.position = originalPos;
            }
        }

        /// <summary>
        /// Update the piece on every frame
        /// </summary>
        void Update()
        {
            // Debug.Log(anim);
            // anim.SetFloat("vertical", Input.GetAxis("Vertical"));
            // anim.SetFloat("horizontal", Input.GetAxis("Horizontal"));
        }

    }
}
