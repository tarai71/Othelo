using UnityEngine;
using System.Collections;

public class inputMouse : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire1")) {
			Vector3 screenPoint = Input.mousePosition;		
			screenPoint.z = 10;
 			Vector3 v = Camera.main.ScreenToWorldPoint(screenPoint);
			float key_x = Mathf.Floor(v.x) + 4.0f;
			float key_y = Mathf.Floor(v.z) + 4.0f;
			GameObject.FindWithTag("GameController").SendMessage("putPiece", new Vector2(key_x, key_y));
		}
	
	}
}
