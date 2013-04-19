using UnityEngine;
using System.Collections;

public class GameMaste : MonoBehaviour {

int[,] board = new int[,]{
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0],
    new int[0,0,0,0,0,0,0,0]
};
var piecePrefab : GameObject;
private var pieceList : GameObject[,] = new GameObject[8,8];
private var pieceType : int = 0;
private var white : int = 0;
private var black : int = 0;
private var gamestatus : String = 'play';
var labelStyleScore : GUIStyle;
var labelStylePieceType : GUIStyle;
var labelStyleGameOver : GUIStyle;








function Start () {
    pieceType = 2;
    
    putPiece(Vector2(3,3));
    pieceType = 1;
	
	putPiece(Vector2(3,4));
	pieceType = 1;
	putPiece(Vector2(4,3));
	pieceType = 2;
	putPiece(Vector2(4,4));
}

// 石を板に置く
function putPiece(key:Vector2) {
	if (key.x < 0 || key.y < 0 || key.x > 7 || key.y > 7) {
		return;
	}
	if (board[key.x][key.y] != 0) {
		return;
	}
	board[key.x][key.y] = pieceType;
	var changeFlag = updateBoard(key, true);
	
	// initial position
	var initialFlag = false;
	if (key == Vector2(3,3) || key == Vector2(3,4) || key == Vector2(4,3) || key == Vector2(4,4)) {
		initialFlag = true;
	}
	if (changeFlag || initialFlag) {
		calcStatus();
		var rotation = transform.rotation;
		var position = Vector3(key.x + 0.5, 1, key.y + 0.5);
		if (pieceType == 1) {
			rotation = Quaternion.AngleAxis(180, Vector3(1, 0, 0));
		} else {
			rotation = Quaternion.AngleAxis(0, Vector3(1, 0, 0));
		}
		pieceList[key.x, key.y] = Instantiate(piecePrefab, position, rotation);
		pieceType = pieceType == 1 ? 2 : 1;
		
		// 置くところが無いかチェック
		if (!checkEnablePut() && !initialFlag) {
			pieceType = pieceType == 1 ? 2 : 1;
			// 攻守交代2回してどこも置けなかったらそのゲームは終了
			if (!checkEnablePut() && !initialFlag) {
                Debug.Log('game over');
                gamestatus = 'gameover';
                yield WaitForSeconds(2.0);
                while (!Input.GetButtonDown("Fire1") || Input.touches.Length > 0) 
                    yield;
                Application.LoadLevel("Main");
            }
        } else {
            Debug.Log('cannot put here');
            board[key.x][key.y] = 0;
        }
        
        // for debug
        for (var i=0; i<board.length; i++) {
            var _s = '';
            for (var j=0; j<board[i].length; j++) {
                _s = _s + ' ' + board[i][j];
            }
            Debug.Log(_s);
        }
    }

    // 置ける場所があるかどうか検索
    function checkEnablePut() {
        for (var x=0; x<board.length; x++) {
            for (var y=0; y<board.length; y++) {
                if (board[x][y] == 0 && updateBoard(Vector2(x,y),false)) {
                    return true;
                }
            }
        }
        return false;
    }
    
    // 盤面の更新、updateFlag が false なら
    // その場所に置けるかどうかのチェックだけ。
    function updateBoard(key:Vector2, updateFlag: boolean) {
        var ix = 0; var iy = 0;
        var revList : Array[] = new Array[8];
        var changeFlag = false;
        // horizon
        ix = key.x + 1; iy = key.y;
        revList[0] = new Array();
        while (true) {
            if (ix >= board.Length) {
                revList[0].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[0].push(Vector2(ix, iy));
            } else if (revList[0].length > 0 && board[ix][iy] != 0) {
                changeFlag = true;
                break;
            } else {
                revList[0].clear();
                break;
            }
            ix += 1;
        }

        ix = key.x - 1; iy = key.y;
        revList[1] = new Array();
        while (true) {
            if (ix < 0) {
                revList[1].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[1].push(Vector2(ix,iy));
            } else if (revList[1].length > 0 && board[ix][iy] != 0) {
                changeFlag = true;
                break;
            } else {
                revList[1].clear();
                break;
            }
            ix -= 1;
        }

        // vertical
        ix = key.x; iy = key.y + 1;
        revList[2] = new Array();
        while (true) {
            if (iy >= board.Length) {
                revList[2].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[2].push(Vector2(ix, iy));
            } else if (revList[2].length > 0 && board[ix][iy] != 0) {
                changeFlag = true;
                break;
            } else {
                revList[2].clear();
                break;
            }
            iy += 1;
        }

        ix = key.x; iy = key.y - 1;
        revList[3] = new Array();
        while (true) {
            if (iy < 0) {
                revList[3].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[3].push(Vector2(ix, iy));
            } else if (revList[3].length > 0 && board[ix][iy] != 0) {
                changeFlag = true;
                break;
            } else {
                revList[3].clear();
                break;
            }
            iy -= 1;
        }

        // cross
        ix = key.x + 1; iy = key.y + 1;
        revList[4] = new Array();
        while (true) {
            if (ix >= board.length || iy >= board.length) {
                revList[4].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[4].push(Vector2(ix,iy));
            } else if (revList[4].length > 0 && board[ix][iy] > 0) {
                changeFlag = true;
                break;
            } else {
                revList[4].clear();
                break;
            }
            iy += 1; ix += 1;
        }

        revList[5] = new Array();
        ix = key.x + 1; iy = key.y - 1;
        while (true) {
            if (ix >= board.length || iy < 0 ) {
                revList[5].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[5].push(Vector2(ix,iy));
            } else if (revList[5].length > 0 && board[ix][iy] > 0) {
                changeFlag = true;
                break;
            } else {
                revList[5].clear();
                break;
            }
            ix += 1; iy -= 1;
        }

        revList[6] = new Array();
        ix = key.x - 1; iy = key.y + 1;
        while (true) {
            if (ix < 0 || iy >= board.length) {
                revList[6].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[6].push(Vector2(ix,iy));
            } else if (revList[6].length > 0 && board[ix][iy] > 0) {
                changeFlag = true;
                break;
            } else {
                revList[6].clear();
                break;
            }
            ix -= 1; iy += 1;
        }

        revList[7] = new Array();
        ix = key.x - 1; iy = key.y - 1;
        while (true) {
            if (ix < 0 || iy < 0) {
                revList[7].clear();
                break;
            }
            if (board[ix][iy] > 0 && board[ix][iy] != pieceType) {
                revList[7].push(Vector2(ix,iy));
            } else if (revList[7].length > 0 && board[ix][iy] > 0) {
                changeFlag = true;
                break;
            } else {
                revList[7].clear();
                break;
            }
            ix -= 1; iy -= 1;
        }

        if (changeFlag) {
            if (updateFlag) {
                for (var val: Array in revList) {
                    for (var v: Vector2 in val) {
                        pieceList[v.x, v.y].transform.rotation *= Quaternion.AngleAxis(180, Vector3(1,0,0));
                        board[v.x][v.y] = (board[v.x][v.y] == 1) ? 2 : 1;
                    }
                }
            }
            return true;
        } else {
            return false;
        }
    }

    function calcStatus() {
        var _white = 0;
        var _black = 0;
        for (var x=0; x<board.length; x++) {
            for (var y=0; y<board.length; y++) {
                if (board[x][y] == 1) {
                    _black += 1;
                } else if (board[x][y] == 2) {
                    _white += 1;
                }
            }
        }
        white = _white;
        black = _black;
    }

    function OnGUI() {
        var rect_score : Rect = Rect(0, 0, Screen.width, Screen.height);
        GUI.Label(rect_score, 'WHITE:' + white + '\nBLACK:' + black, labelStyleScore);
        var rect_piece : Rect = Rect(0, 0, Screen.width, Screen.height);
        var piece = pieceType == 1 ? 'black' : 'white';
        GUI.Label(rect_piece, piece, labelStylePieceType);
        var rect_gameover : Rect = Rect(0, Screen.height / 2 - 25, Screen.width, 50);
        if (gamestatus == 'gameover') {
            var result : String = '';
            if (white > black) {
                result = 'white win!';
            } else if (white < black) {
                result = 'black win!';
            } else {
                result = 'draw...';
            }
            GUI.Label(rect_gameover, result, labelStyleGameOver);
        }
    }



}