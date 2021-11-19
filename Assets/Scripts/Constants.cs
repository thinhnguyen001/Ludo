using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameGrid { public int row, col; };

public enum Player { RED,BLUE,YELLOW,GREEN };

public struct GameIndex { public string posType; public int pos; };

public struct GamePiece { public Player player; public int pieceNumber; };

public static class Constants
{
    public const string ROLL_DIE = "ROLL A DIE";
    public const string MOVE_PLAYER = "MOVE THE PLAYER";
    public const string START_POS = "START";
    public const string NORMAL_POS = "NORMAL";
    public const string END_POS = "END";
}
