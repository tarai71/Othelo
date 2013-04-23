using UnityEngine;
using System.Collections;

public class menu : MonoBehaviour
{
	private Rect[] windowRect = {
		new Rect (10, 60, 200, 10),
		new Rect (10, 180, 200, 10),
		new Rect (220, 60, 200, 10),
		new Rect (220, 155, 200, 10)
	};
	private static int[] option = {0,0,0,0};
	private static float[] timeTable = {
		0f, 5f, 10f, 15f, 20f, 25f, 30f
	};

	void OnGUI ()
	{
		windowRect[0] = GUILayout.Window (0, windowRect[0], MakeSelectWindow, StringTable.SENTE);
		windowRect[1] = GUILayout.Window (1, windowRect[1], MakeSelectWindow, StringTable.GOTE);
		windowRect[2] = GUILayout.Window (2, windowRect[2], MakeGuideWindow, StringTable.GUIDE);
		windowRect[3] = GUILayout.Window (3, windowRect[3], MakeTimeWindow, StringTable.TIME);

		GUILayout.BeginArea( new Rect (10, 10, 410, 40));
			GUILayout.Space(10);
			if(GUILayout.Button(StringTable.START)) {
				this.enabled = false;
				Application.LoadLevel("Main");
		    }
		GUILayout.EndArea();	
	}
	
	void Awake ()
	{
		DontDestroyOnLoad (this);
	}
	
	void MakeSelectWindow (int id)
	{
		GUILayout.Space (10);
		option[id] = GUILayout.SelectionGrid(option[id], new string[]{StringTable.HUMAN,StringTable.COMPUTER,StringTable.NETWORK}, 1);
		GUILayout.FlexibleSpace ();
	}

	void MakeGuideWindow (int id)
	{
		GUILayout.Space (10);
		option[id] = GUILayout.SelectionGrid(option[id], new string[]{StringTable.ON,StringTable.OFF}, 1);
		GUILayout.FlexibleSpace ();
	}

	void MakeTimeWindow (int id)
	{
		GUILayout.Space (10);
		option[id] = GUILayout.SelectionGrid(option[id], new string[]{StringTable.NO_LIMIT,StringTable.SEC5,StringTable.SEC10,StringTable.SEC15,StringTable.SEC20,StringTable.SEC25,StringTable.SEC30}, 1);
		GUILayout.FlexibleSpace ();
	}

	public static bool getGuideEnable () 
	{
		return (option[2] == 0);
	}

	public static float getLimitTime () 
	{
		return timeTable[option[3]];
	}
}
