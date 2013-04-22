using UnityEngine;
using System.Collections;

public class main : MonoBehaviour {
	
	enum GAME_STATUS {
		PLAY = 0,
		GAMEOVER
	}
	
	enum PIECE_TYPE {
		EMPTY = 0,
		BLACK,
		WHITE
	}
	
	public GameObject piecePrefab;
	public GameObject markerPrefab;
	public GUIStyle labelStyleScoreBlack;
	public GUIStyle labelStyleScoreWhite;
	public GUIStyle labelStyleGameOver;
	
	PIECE_TYPE[,] board = new PIECE_TYPE[8,8];
	GameObject[,] pieceList = new GameObject[8,8];
	PIECE_TYPE pieceType = 0;
	int white = 0;
	int black = 0;
	GAME_STATUS gamestatus = GAME_STATUS.PLAY;
	ArrayList markerList = new ArrayList();
		
	// Use this for initialization
	void Start () {
		for (int i=0; i<board.GetLength(0); i++) {
			for (int j=0; j<board.GetLength(1); j++) {
				board[i,j] = 0;
			}
		}
		pieceType = PIECE_TYPE.WHITE; putPiece(new Vector2(3,4));
		pieceType = PIECE_TYPE.BLACK; putPiece(new Vector2(3,3));
		pieceType = PIECE_TYPE.BLACK; putPiece(new Vector2(4,4));
		pieceType = PIECE_TYPE.WHITE; putPiece(new Vector2(4,3));
	
	}
	
	// Update is called once per frame
	void Update () {
		byte alfa;
		alfa = (Time.time % 1.0f < 0.5f)? (byte)((Time.time % 1.0f) * 255 + 127) : (byte)((1.0f - Time.time % 1.0f) * 255 + 127);
		
		foreach(GameObject obj in markerList) {
			if(obj) {
				obj.renderer.material.color = new Color32(209,221, 48,alfa);
			}
		}
		
		if(pieceType == PIECE_TYPE.BLACK) {
			labelStyleScoreBlack.normal.textColor = new Color32(209,221, 48,alfa);
			labelStyleScoreWhite.normal.textColor = new Color32(193,193,193,255);
		} else {
			labelStyleScoreBlack.normal.textColor = new Color32(193,193,193,255);
			labelStyleScoreWhite.normal.textColor = new Color32(209,221, 48,alfa);
		}
		
	}


	// 駒を盤に置く/
	void putPiece(Vector2 key)
	{
		if (key.x < 0 || key.y < 0 || key.x > 7 || key.y > 7) {
			return;
		}
		if (board[(int)key.x,(int)key.y] != 0) {
			return;
		}
		if (gamestatus != GAME_STATUS.PLAY) {
			return;
		}
			
		board[(int)key.x,(int)key.y] = pieceType;
		bool changeFlag = updateBoard(key, true);
	
		// initial position
		var initialFlag = false;
		if (key == new Vector2(3,3) || key == new Vector2(3,4) || key == new Vector2(4,3) || key == new Vector2(4,4)) {
			initialFlag = true;
		}
		if (changeFlag || initialFlag) {
			calcStatus();
			var rotation = transform.rotation;
			var position = new Vector3(key.x -4.0f + 0.5f, 0.25f, key.y -4.0f + 0.5f);
			//if (pieceType == 1) {
			//	rotation = Quaternion.AngleAxis(180, new Vector3(1, 0, 0));
			//} else {
			//	rotation = Quaternion.AngleAxis(0, new Vector3(1, 0, 0));
			//}
			pieceList[(int)key.x, (int)key.y] = (GameObject)Instantiate(piecePrefab, position, rotation);
			for (int i=0; i<board.GetLength(0); i++) {
				for (int j=0; j<board.GetLength(1); j++) {
					if (board[i,j] == PIECE_TYPE.BLACK) {
						pieceList[i, j].renderer.material.color = new Color(0,0,0,255);
					} else if (board[i,j] == PIECE_TYPE.WHITE) {
						pieceList[i, j].renderer.material.color = new Color(255,255,255,255);
					}
				}
			}
			pieceType = pieceType == PIECE_TYPE.BLACK ? PIECE_TYPE.WHITE : PIECE_TYPE.BLACK;
			// 置くところが無いかチェック/
			foreach(Object obj in markerList) {
				Object.Destroy(obj);
			}
			ArrayList enablePutList = new ArrayList();
			if (!checkEnablePut(ref enablePutList) && !initialFlag) {
				pieceType = pieceType == PIECE_TYPE.BLACK ? PIECE_TYPE.WHITE : PIECE_TYPE.BLACK;
				// 攻守交代2回してどこも置けなかったらそのゲームは終了/
				if (!checkEnablePut(ref enablePutList) && !initialFlag) {
					StartCoroutine("GameOver");
				}
			}
			foreach(Vector2 v in enablePutList) {
				position = new Vector3(v.x -4.0f + 0.5f, 0.201f, v.y -4.0f + 0.5f);
				markerList.Add(Instantiate(markerPrefab, position, Quaternion.identity));
			}
			enablePutList.Clear();
		} else {
			Debug.Log("cannot put here");
			board[(int)key.x,(int)key.y] = 0;
		}
	}
	IEnumerator GameOver() {
		Debug.Log("game over");
		gamestatus = GAME_STATUS.GAMEOVER;
		yield return new WaitForSeconds(2.0f);
		//while (!Input.GetButtonDown("Fire1") || Input.touches.Length > 0) yield return;

		Application.LoadLevel("Main");
	}
	

	// 置ける場所があるかどうか検索/
	bool checkEnablePut(ref ArrayList list) {
		for (int x=0; x<board.GetLength(0); x++) {
			for (int y=0; y<board.GetLength(1); y++) {
				if (board[x,y] == 0 && updateBoard(new Vector2(x,y),false)) {
					list.Add(new Vector2(x,y));
				}
			}
		}
		return (list.Count > 0);
	}

	// 盤面の更新、updateFlag が false なら/
	// その場所に置けるかどうかのチェックだけ/
	bool updateBoard(Vector2 key, bool updateFlag) {
		int ix = 0; int iy = 0;
		
		ArrayList[] revList = new ArrayList[8];
		var changeFlag = false;
		// horizon
		ix = (int)key.x + 1; iy = (int)key.y;
		revList[0] = new ArrayList();
		while (true) {
			if (ix >= board.GetLength(0)) {
				revList[0].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[0].Add(new Vector2(ix, iy));
			} else if (revList[0].Count > 0 && board[ix,iy] != 0) {
				changeFlag = true;
				break;
			} else {
				revList[0].Clear();
				break;
			}
			ix += 1;
		}
		
		ix = (int)key.x - 1; iy = (int)key.y;
		revList[1] = new ArrayList();
		while (true) {
			if (ix < 0) {
				revList[1].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[1].Add(new Vector2(ix,iy));
			} else if (revList[1].Count > 0 && board[ix,iy] != 0) {
				changeFlag = true;
				break;
			} else {
				revList[1].Clear();
				break;
			}
			ix -= 1;
		}
	
		// vertical
		ix = (int)key.x; iy = (int)key.y + 1;
		revList[2] = new ArrayList();
		while (true) {
			if (iy >= board.GetLength(1)) {
				revList[2].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[2].Add(new Vector2(ix, iy));
			} else if (revList[2].Count > 0 && board[ix,iy] != 0) {
				changeFlag = true;
				break;
			} else {
				revList[2].Clear();
				break;
			}
			iy += 1;
		}
	
		ix = (int)key.x; iy = (int)key.y - 1; 
		revList[3] = new ArrayList();
		while (true) {
			if (iy < 0) {
				revList[3].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[3].Add(new Vector2(ix, iy));
			} else if (revList[3].Count > 0 && board[ix,iy] != 0) {
				changeFlag = true;
				break;
			} else {
				revList[3].Clear();
				break;
			}
			iy -= 1;
		}
	
		// cross
		ix = (int)key.x + 1; iy = (int)key.y + 1;
		revList[4] = new ArrayList();
		while (true) {
			if (ix >= board.GetLength(0) || iy >= board.GetLength(1)) {
				revList[4].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[4].Add(new Vector2(ix,iy));
			} else if (revList[4].Count > 0 && board[ix,iy] != PIECE_TYPE.EMPTY) {
				changeFlag = true;
				break;
			} else {
				revList[4].Clear();
				break;
			}
			iy += 1; ix += 1;
		}
	
		revList[5] = new ArrayList();
		ix = (int)key.x + 1; iy = (int)key.y - 1;
		while (true) {
			if (ix >= board.GetLength(0) || iy < 0 ) {
				revList[5].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[5].Add(new Vector2(ix,iy));
			} else if (revList[5].Count > 0 && board[ix,iy] != PIECE_TYPE.EMPTY) {
				changeFlag = true;
				break;
			} else {
				revList[5].Clear();
				break;
			}
			ix += 1; iy -= 1;
		}
		
	
		revList[6] = new ArrayList();
		ix = (int)key.x - 1; iy = (int)key.y + 1;
		while (true) {
			if (ix < 0 || iy >= board.GetLength(1)) {
				revList[6].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[6].Add(new Vector2(ix,iy));
			} else if (revList[6].Count > 0 && board[ix,iy] != PIECE_TYPE.EMPTY) {
				changeFlag = true;
				break;
			} else {
				revList[6].Clear();
				break;
			}
			ix -= 1; iy += 1;
		}
	
		revList[7] = new ArrayList();
		ix = (int)key.x - 1; iy = (int)key.y - 1;
		while (true) {
			if (ix < 0 || iy < 0) {
				revList[7].Clear();
				break;
			}
			if (board[ix,iy] != PIECE_TYPE.EMPTY && board[ix,iy] != pieceType) {
				revList[7].Add(new Vector2(ix,iy));
			} else if (revList[7].Count > 0 && board[ix,iy] != PIECE_TYPE.EMPTY) {
				changeFlag = true;
				break;
			} else {
				revList[7].Clear();
				break;
			}
			ix -= 1; iy -= 1;
		}
	
		if (changeFlag) {
			if (updateFlag) {
				foreach (ArrayList val in revList) {
					foreach (Vector2 v in val) {
						pieceList[(int)v.x, (int)v.y].transform.rotation *= Quaternion.AngleAxis(180, new Vector3(1,0,0));
						board[(int)v.x, (int)v.y] = (board[(int)v.x, (int)v.y] == PIECE_TYPE.BLACK) ? PIECE_TYPE.WHITE : PIECE_TYPE.BLACK;
					} 
				}
			}
			return true;
		} else {
			return false;
		}
	}

	void calcStatus() {
		int _white = 0;
		int _black = 0;
		for (int x=0; x<board.GetLength(0); x++) {
			for (int y=0; y<board.GetLength(1); y++) {
				if (board[x,y] == PIECE_TYPE.BLACK) {
					_black += 1;
				} else if (board[x,y] == PIECE_TYPE.WHITE) {
					_white += 1;
				}
			}
		}
		white = _white;
		black = _black;
	}
	
	void OnGUI() {
		GUI.Box(new Rect(10,10,100,80), "BLACK");
		GUI.Label(new Rect(10,10,100,80), black.ToString("d2"), labelStyleScoreBlack);
		GUI.Box(new Rect(10,100,100,80), "WHITE");
		GUI.Label(new Rect(10,100,100,80), white.ToString("d2"), labelStyleScoreWhite);

		Rect rect_gameover = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 50, 600, 100);
		if (gamestatus == GAME_STATUS.GAMEOVER) {
			string result = "";
			if (white > black) {
				result = "WHITE WON!";
			} else if (white < black) {
				result = "BLACK WON!";
			} else {
				result = "DRAW";
			}
			GUI.Box(rect_gameover, "");
			GUI.Box(rect_gameover, "");
			GUI.Label(rect_gameover, result, labelStyleGameOver);
		}

	}
}
