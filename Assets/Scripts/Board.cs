using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    Dictionary<GamePiece, int> position;
    List<Player> players;
    List<GamePiece> moveablePieces;

    readonly List<GameIndex> common = new List<GameIndex>()
    {
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 0},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 1},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 2},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 3},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 4},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 5},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 6},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 7},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 8},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 9},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 10},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 11},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 12},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 13},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 14},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 15},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 16},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 17},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 18},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 19},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 20},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 21},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 22},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 23},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 24},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 25},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 26},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 27},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 28},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 29},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 30},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 31},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 32},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 33},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 34},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 35},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 36},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 37},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 38},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 39},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 40},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 41},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 42},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 43},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 44},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 45},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 46},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 47},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 48},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 49},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 50},
        new GameIndex() {  posType = Constants.NORMAL_POS , pos = 51}
    };

    readonly List<GameIndex> end = new List<GameIndex>()
    {
        new GameIndex() {  posType = Constants.END_POS , pos = 0},
        new GameIndex() {  posType = Constants.END_POS , pos = 1},
        new GameIndex() {  posType = Constants.END_POS , pos = 2},
        new GameIndex() {  posType = Constants.END_POS , pos = 3},
        new GameIndex() {  posType = Constants.END_POS , pos = 4},
        new GameIndex() {  posType = Constants.END_POS , pos = 5}
    };

    Dictionary<Player, List<GameIndex>> playerPositions;

    Dictionary<int, List<GameIndex>> results;


    public Board()
    {
        playerPositions = new Dictionary<Player, List<GameIndex>>();
        players = new List<Player>();
        position = new Dictionary<GamePiece, int>();
        results = new Dictionary<int, List<GameIndex>>();

        for (int i = 0; i < (PlayerPrefs.HasKey("players") ? PlayerPrefs.GetInt("players") : 4); i++)
        {
            players.Add((Player)i);
            List<GameIndex> temp = new List<GameIndex>();
            temp.Add(new GameIndex() { posType = Constants.START_POS, pos = 0 });
            for (int k = 0; k < 51; k++)
            {
                temp.Add(common[(k + 13 * i) % common.Count]);
            }
            for (int k = 0; k < end.Count; k++)
            {
                temp.Add(end[k]);
            }
            playerPositions[(Player)i] = temp;
            for (int j = 0; j < 4; j++)
            {
                position[new GamePiece() { player = (Player)i, pieceNumber = j }] = 0;
            }
        }
    }

    public List<GamePiece> GetRoll(Player currentPlayer,int roll)
    {
        moveablePieces = new List<GamePiece>();
        for (int i = 0; i < 4; i++)
        {
            int currentPos = position[new GamePiece { player = currentPlayer, pieceNumber = i }];
            if(currentPos == 0)
            {
                if(roll == 6)
                {
                    moveablePieces.Add(new GamePiece() { player = currentPlayer, pieceNumber = i });
                    results[i] = new List<GameIndex>() { playerPositions[currentPlayer][1] };
                }
            }
            else
            {
                if(currentPos + roll < playerPositions[currentPlayer].Count)
                {
                    moveablePieces.Add(new GamePiece() { player = currentPlayer, pieceNumber = i });
                    List<GameIndex> temp = new List<GameIndex>();
                    for (int j = currentPos + 1; j <= currentPos + roll; j++)
                    {
                        temp.Add(playerPositions[currentPlayer][j]);
                    }
                    results[i] = temp;
                }
            }
        }
        return moveablePieces;
    }

    public List<GameIndex> UpdateRoll(GamePiece currentPiece,int roll)
    {
        position[currentPiece] += (roll == 6 && position[currentPiece] == 0) ? 1 : roll;
        return results[currentPiece.pieceNumber];
    }

    public void UpdateKill(GamePiece piece)
    {
        position[piece] = 0;
    }

    public List<GamePiece> GetPieceAtIndex(GameIndex currentIndex)
    {
        List<GamePiece> pieces = new List<GamePiece>();
        foreach (KeyValuePair<GamePiece,int> pair in position)
        {
            GameIndex currentPiecePosition = playerPositions[pair.Key.player][pair.Value];
            if(currentPiecePosition.posType == currentIndex.posType && currentPiecePosition.pos == currentIndex.pos)
            {
                pieces.Add(pair.Key);
            }
        }
        return pieces;
    }
}
