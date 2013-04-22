using UnityEngine;
using System.Collections;

public class main : MonoBehaviour {

	public GameObject piecePrefab;
	public GameObject markerPrefab;
	public GUIStyle labelStyleScore;
	public GUIStyle labelStylePieceType;
	public GUIStyle labelStyleGameOver;
	
	enum STATUS {
		PLAY = 0,
		GAMEOVER
	}

	enum PIECE {
		EMPTY = 0,
		BLACK,
		WHITE,
	}
	
	STATUS gamestatus = STATUS.PLAY;
	PIECE[,] board = new PIECE[8,8];
	GameObject[,] pieceList = new GameObject[8,8];
	PIECE pieceType = PIECE.EMPTY;
	int white = 0;
	int black = 0;
	ArrayList markerList = new ArrayList();
		
	// Use this for initialization
	void Start () {
		for (int x=0; x<board.GetLength(0); x++) {
			for (int y=0; y<board.GetLength(1); y++) {
				board[x,y] = PIECE.EMPTY;	
			}
		}
		pieceType = PIECE.BLACK; putPiece(new Vector2(3,4));
		pieceType = PIECE.WHITE; putPiece(new Vector2(3,3));
//		pieceType = PIECE.WHITE; putPiece(new Vector2(4,4));
		pieceType = PIECE.BLACK; putPiece(new Vector2(4,3));
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 駒を盤に置く/
	void putPiece(Vector2 key)
	{
		if (key.x < 0 || key.y < 0 || key.x > 7 || key.y > 7) {
			return;
		}
		if (board[(int)key.x,(int)key.y] != PIECE.EMPTY) {
			return;
		}
		if (gamestatus != STATUS.PLAY) {
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
					if (board[i,j] == PIECE.BLACK) {
						pieceList[i, j].renderer.material.color = new Color(0,0,0,255);
					} else if (board[i,j] == PIECE.WHITE) {
						pieceList[i, j].renderer.material.color = new Color(255,255,255,255);
					}
				}
			}
			pieceType = pieceType == PIECE.WHITE ? PIECE.BLACK : PIECE.WHITE;
			// 置くところが無いかチェック/
			foreach(Object obj in markerList) {
				Object.Destroy(obj);
			}
			ArrayList enablePutList = new ArrayList();
			if (!checkEnablePut(ref enablePutList) && !initialFlag) {
				pieceType = pieceType == PIECE.WHITE ? PIECE.BLACK : PIECE.WHITE;
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
			board[(int)key.x,(int)key.y] = PIECE.EMPTY;
		}

		// for debug
		string _s = "";
		for (int i=0; i<board.GetLength(0); i++) {
			_s = _s + '\n';
			for (int j=0; j<board.GetLength(1); j++) {
				if(board[i,j] == PIECE.BLACK ) {
					_s = _s + 'B';
				} else if(board[i,j] == PIECE.WHITE ) {
					_s = _s + 'W';
				} else {
					_s = _s + 'E';
				}
			}
		}
		Debug.Log(_s);
}
	
	IEnumerator GameOver() {
		Debug.Log("game over");
		gamestatus = STATUS.GAMEOVER;
		yield return new WaitForSeconds(2.0f);
		//while (!Input.GetButtonDown("Fire1") || Input.touches.Length > 0) yield return;

		Application.LoadLevel("Main");
	}
	

	// 置ける場所があるかどうか検索/
	bool checkEnablePut(ref ArrayList list) {
		for (int x=0; x<board.GetLength(0); x++) {
			for (int y=0; y<board.GetLength(1); y++) {
				if (board[x,y] == PIECE.EMPTY && updateBoard(new Vector2(x,y),false)) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[0].Add(new Vector2(ix, iy));
			} else if (revList[0].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[1].Add(new Vector2(ix,iy));
			} else if (revList[1].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[2].Add(new Vector2(ix, iy));
			} else if (revList[2].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[3].Add(new Vector2(ix, iy));
			} else if (revList[3].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[4].Add(new Vector2(ix,iy));
			} else if (revList[4].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[5].Add(new Vector2(ix,iy));
			} else if (revList[5].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[6].Add(new Vector2(ix,iy));
			} else if (revList[6].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
			if (board[ix,iy] != PIECE.EMPTY && board[ix,iy] != pieceType) {
				revList[7].Add(new Vector2(ix,iy));
			} else if (revList[7].Count > 0 && board[ix,iy] != PIECE.EMPTY) {
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
						board[(int)v.x, (int)v.y] = (board[(int)v.x, (int)v.y] == PIECE.WHITE) ? PIECE.BLACK : PIECE.WHITE;
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
				if (board[x,y] == PIECE.BLACK) {
					_black += 1;
				} else if (board[x,y] == PIECE.WHITE) {
					_white += 1;
				}
			}
		}
		white = _white;
		black = _black;
	}
	
	void OnGUI() {
		Rect rect_score = new Rect(0, 0, Screen.width, Screen.height);
		GUI.Label(rect_score, "WHITE:" + white + "\nBLACK:" + black, labelStyleScore);
		
		Rect rect_piece = new Rect(0, 30, Screen.width, Screen.height);
		var piece = pieceType == PIECE.WHITE ? "black" : "white";
		GUI.Label(rect_piece, piece, labelStylePieceType);
		
		Rect rect_gameover = new Rect(0, Screen.height / 2 - 25, Screen.width, 50);
		if (gamestatus == STATUS.GAMEOVER) {
			string result = "";
			if (white > black) {
				result = "white won!";
			} else if (white < black) {
				result = "black won!";
			} else {
				result = "draw...";
			}
			GUI.Label(rect_gameover, result, labelStyleGameOver);
		}
	}
}
