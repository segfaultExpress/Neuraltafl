using System;
using UnityEngine;

using NeuralTaflGame;

namespace Unitafl
{
	public class SquareU
	{

		public GameObject squareObj { get; set; }

		public GameObject ghostPiece { get; set; }

		public Vector3 loc { get; set; }
		public Vector2 grid { get; set; }

		private Material fireMaterial;
		private Material fireChosenMaterial;

		/// <summary>
		/// A square is a placeable location for a piece, and handles effects, piece data, etc.
		/// </summary>
		/// <param name="loc">The physical location of the game object in the engine (i.e. (1.23, 3.42, -3.11))</param>
		/// <param name="grid">The game-level array location of the piece in the game (i.e. (2, 5))</param>
		/// <param name="hdrp">Hack solution for the debugging between universal vs hdrp pipelines</param>
		public SquareU(Vector3 loc, Vector2 grid, bool hdrp = true) // TODO: allow different pipelines
		{
			Debug.Log("square made");

			this.loc = loc;
			this.grid = grid;

			string resPath = "Materials/" + (hdrp ? "hdrp/" : "WebGL/");
			
        	this.fireMaterial = Resources.Load(resPath + "Fire", typeof(Material)) as Material;
        	this.fireChosenMaterial = Resources.Load(resPath + "FireChosen", typeof(Material)) as Material;
		}

		/// <summary>
		/// Toggle activation for the square, whatever that means (color change, highlight, etc) 
		/// </summary>
		/// <param name="isActive">Bool to set active or inactive</param>
		public void activate(bool isActive = true)
		{
			squareObj.transform.Find("Walls").gameObject.SetActive(isActive);
		}

		/// <summary>
		/// Level 2 "activation", if the piece is hovering over it should be activated more
		/// </summary>
		/// <param name="isActive">Bool to set active or inactive</param>
		public void highlight(bool isActive = true)
		{
			foreach (Transform child in squareObj.GetComponentsInChildren<Transform>(true))
			{
				Renderer rend = child.GetComponent<Renderer>();
				if (rend != null)
					rend.material = (isActive ? fireChosenMaterial : fireMaterial);
			}
		}
	}
}