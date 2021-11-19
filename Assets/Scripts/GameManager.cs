using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> playerNames;

    [SerializeField]
    GameObject gamePiece;

    public bool canClick, hasGameFinished;

    Board myBoard;

    public static GameManager instance;

    string gameState;
    int roll;
    List<GamePiece> result;
    bool hasKilled, hasReachedEnd;

    int numOfPlayers;
    List<Player> players;
    int currentPlayer;
    Dictionary<GamePiece, GameObject> gamePieces;

    public readonly Dictionary<Player, List<Vector3>> startPos = new Dictionary<Player, List<Vector3>>()
    {
        {
            Player.RED,
            new List<Vector3>()
            {
                new Vector3(2f,-2f,-1f),new Vector3(4f,-2f,-1f),new Vector3(2f,-4f,-1f),new Vector3(4f,-4f,-1f)
            }
        },
        {
            Player.BLUE,
            new List<Vector3>()
            {
                new Vector3(11f,-2f,-1f),new Vector3(13f,-2f,-1f),new Vector3(11f,-4f,-1f),new Vector3(13f,-4f,-1f)
            }
        },
        {
            Player.YELLOW,
            new List<Vector3>()
            {
                new Vector3(11f,-11f,-1f),new Vector3(13f,-11f,-1f),new Vector3(11f,-13f,-1f),new Vector3(13f,-13f,-1f)
            }
        },
        {
            Player.GREEN,
            new List<Vector3>()
            {
                new Vector3(2f,-11f,-1f),new Vector3(4f,-11f,-1f),new Vector3(2f,-13f,-1f),new Vector3(4f,-13f,-1f)
            }
        }
    };

    List<GameIndex> safePositions = new List<GameIndex>()
    {
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 0 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 13 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 26 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 39 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 9 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 22 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 35 },
        new GameIndex() { posType = Constants.NORMAL_POS, pos = 48 }
    };
    Dictionary<GameGrid, GameIndex> gridToIndex;
    Dictionary<GameIndex, GameGrid> indexToGrid;
    Dictionary<Player, Dictionary<GameIndex, GameGrid>> indexToGridEnd;
    Dictionary<Player, int> remainingPieces;
    Dictionary<int, Player> playerRank;

    public delegate void UpdateMessage(Player temp);
    public event UpdateMessage Message;

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void GameRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        canClick = true;
        hasGameFinished = false;
        gameState = Constants.ROLL_DIE;
        currentPlayer = 0;

        numOfPlayers = PlayerPrefs.HasKey("players") ? PlayerPrefs.GetInt("players") : 4;

        myBoard = new Board();
        players = new List<Player>();
        gamePieces = new Dictionary<GamePiece, GameObject>();
        remainingPieces = new Dictionary<Player, int>();
        playerRank = new Dictionary<int, Player>();
        gridToIndex = new Dictionary<GameGrid, GameIndex>();
        indexToGrid = new Dictionary<GameIndex, GameGrid>();
        indexToGridEnd = new Dictionary<Player, Dictionary<GameIndex, GameGrid>>();

        for (int i = 0; i < numOfPlayers; i++)
        {
            players.Add((Player)i);
            remainingPieces[(Player)i] = 4;
            playerNames[i].SetActive(true);
            playerNames[i].GetComponent<Text>().text =
                PlayerPrefs.HasKey((i + 1).ToString()) ? PlayerPrefs.GetString((i + 1).ToString()) : "Player" + (i + 1);

            for (int j = 0; j < 4; j++)
            {
                GamePiece temp = new GamePiece() { player = (Player)i, pieceNumber = j };
                GameObject piece = Instantiate(gamePiece);
                piece.transform.position = startPos[(Player)i][j];
                piece.GetComponent<Piece>().SetColor((Player)i);
                gamePieces[temp] = piece;
            }
        }
    }

    private void Update()
    {
        if (!canClick || hasGameFinished) return;

        if(Input.GetMouseButtonDown(0))
        {
            
            switch (gameState)
            {
                case Constants.ROLL_DIE:
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
                    RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                    if (!hit) return;

                    if (hit.collider.CompareTag("Die"))
                    {
                        gameState = Constants.MOVE_PLAYER;
                        canClick = false;
                        roll = Random.Range(0, 6) + 1;
                        hit.collider.gameObject.GetComponent<Die>().RollDie(roll);
                        result = myBoard.GetRoll(players[currentPlayer], roll);
                    }

                    break;

                case Constants.MOVE_PLAYER:
                    mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    GameGrid currentGrid = TransformToGrid(mousePos);
                    GameIndex currentIndex = GridToIndex(currentGrid);
                    if (currentIndex.pos == -1) return;

                    List<GamePiece> selectedPieces = myBoard.GetPieceAtIndex(currentIndex);
                    GamePiece movingPiece = GetPieceCommon(selectedPieces, result);
                    if (movingPiece.pieceNumber == -1) return;

                    List<GameIndex> indexes = myBoard.UpdateRoll(movingPiece, roll);
                    List<GameGrid> resultGrid = IndexToGrid(indexes);
                    List<Vector3> resultVectors = ListToVector(resultGrid);
                    gamePieces[movingPiece].GetComponent<Piece>().SetPos(resultVectors);

                    hasKilled = false;
                    GameIndex lastIndex = indexes[indexes.Count - 1];
                    if(lastIndex.posType == Constants.NORMAL_POS && !ContainsSafe(lastIndex))
                    {
                        List<GamePiece> removingPieces = myBoard.GetPieceAtIndex(lastIndex);
                        foreach(GamePiece remove in removingPieces)
                        {
                            if(remove.player != players[currentPlayer])
                            {
                                gamePieces[remove].transform.position = startPos[remove.player][remove.pieceNumber];
                                myBoard.UpdateKill(remove);
                                hasKilled = true;
                            }
                        }
                    }


                    hasReachedEnd = false;
                    if (lastIndex.posType == Constants.END_POS && lastIndex.pos == 5)
                    {
                        hasReachedEnd = true;
                        remainingPieces[players[currentPlayer]] -= 1;
                        if(remainingPieces[players[currentPlayer]] == 0)
                        {
                            playerNames[(int)players[currentPlayer]].GetComponent<Text>().text =
                                (playerRank.Count + 1).ToString() + ". " +
                                playerNames[(int)players[currentPlayer]].GetComponent<Text>().text;
                            playerRank[playerRank.Count + 1] = players[currentPlayer];
                            players.RemoveAt(currentPlayer);

                            if(players.Count == 1)
                            {
                                hasGameFinished = true;
                            }
                        }

                        currentPlayer %= players.Count;
                    }

                    break;
                default:
                    break;
            }
        }        
    }

    bool ContainsSafe(GameIndex index)
    {
        foreach(GameIndex temp in safePositions)
        {
            if(temp.pos ==  index.pos && temp.posType == index.posType)
            {
                return true;
            }
        }
        return false;
    }

    List<Vector3> ListToVector(List<GameGrid> gridList)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < gridList.Count; i++)
        {
            result.Add(GridToTransform(gridList[i]));
        }
        return result;
    }

    List<GameGrid> IndexToGrid(List<GameIndex> indexes)
    {
        List<GameGrid> result = new List<GameGrid>();
        for (int i = 0; i < indexes.Count; i++)
        {
            result.Add(IndexToGrid(indexes[i]));
        }
        return result;
    }

    GameGrid IndexToGrid(GameIndex index)
    {
        if (index.posType == Constants.NORMAL_POS) return indexToGrid[index];
        return indexToGridEnd[players[currentPlayer]][index];
    }

    GamePiece GetPieceCommon(List<GamePiece> selected,List<GamePiece> result)
    {
        foreach (GamePiece select in selected)
        {
            foreach(GamePiece resultPiece in result)
            {
                if(select.pieceNumber == resultPiece.pieceNumber && select.player == resultPiece.player)
                {
                    return resultPiece;
                }
            }
        }
        return new GamePiece() { pieceNumber = -1, player = Player.BLUE };
    }

    public void RollEnd()
    {
        if(result.Count == 0)
        {
            gameState = Constants.ROLL_DIE;
            currentPlayer++;
            currentPlayer %= players.Count;
            Message(players[currentPlayer]);
        }
        canClick = true;
    }

    public void MoveEnd()
    {
        gameState = Constants.ROLL_DIE;
        canClick = true;
        if (roll == 6 || hasKilled || hasReachedEnd) return;
        currentPlayer++;
        currentPlayer %= players.Count;
        Message(players[currentPlayer]);
    }

    GameIndex GridToIndex(GameGrid grid)
    {
        Dictionary<GameGrid, GameIndex> temp = new Dictionary<GameGrid, GameIndex>()
        {
            { new GameGrid() { row= 6,col = 1}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 0 } },
            { new GameGrid() { row= 6,col = 2}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 1 } },
            { new GameGrid() { row= 6,col = 3}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 2 } },
            { new GameGrid() { row= 6,col = 4}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 3 } },
            { new GameGrid() { row= 6,col = 5}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 4 } },
            { new GameGrid() { row= 5,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 5 } },
            { new GameGrid() { row= 4,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 6 } },
            { new GameGrid() { row= 3,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 7 } },
            { new GameGrid() { row= 2,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 8 } },
            { new GameGrid() { row= 1,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 9 } },
            { new GameGrid() { row= 0,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 10 } },
            { new GameGrid() { row= 0,col = 7}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 11 } },
            { new GameGrid() { row= 0,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 12 } },
            { new GameGrid() { row= 1,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 13 } },
            { new GameGrid() { row= 2,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 14 } },
            { new GameGrid() { row= 3,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 15 } },
            { new GameGrid() { row= 4,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 16 } },
            { new GameGrid() { row= 5,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 17 } },
            { new GameGrid() { row= 6,col = 9}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 18 } },
            { new GameGrid() { row= 6,col = 10}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 19 } },
            { new GameGrid() { row= 6,col = 11}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 20 } },
            { new GameGrid() { row= 6,col = 12}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 21 } },
            { new GameGrid() { row= 6,col = 13}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 22 } },
            { new GameGrid() { row= 6,col = 14}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 23 } },
            { new GameGrid() { row= 7,col = 14}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 24 } },
            { new GameGrid() { row= 8,col = 14}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 25 } },
            { new GameGrid() { row= 8,col = 13}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 26 } },
            { new GameGrid() { row= 8,col = 12}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 27 } },
            { new GameGrid() { row= 8,col = 11}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 28 } },
            { new GameGrid() { row= 8,col = 10}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 29 } },
            { new GameGrid() { row= 8,col = 9 }, new GameIndex() { posType = Constants.NORMAL_POS,pos = 30 } },
            { new GameGrid() { row= 9 ,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 31 } },
            { new GameGrid() { row= 10,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 32 } },
            { new GameGrid() { row= 11,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 33 } },
            { new GameGrid() { row= 12,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 34 } },
            { new GameGrid() { row= 13,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 35 } },
            { new GameGrid() { row= 14,col = 8}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 36 } },
            { new GameGrid() { row= 14,col = 7}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 37 } },
            { new GameGrid() { row= 14,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 38 } },
            { new GameGrid() { row= 13,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 39 } },
            { new GameGrid() { row= 12,col = 6 }, new GameIndex() { posType = Constants.NORMAL_POS,pos = 40 } },
            { new GameGrid() { row= 11,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 41 } },
            { new GameGrid() { row= 10,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 42 } },
            { new GameGrid() { row= 9 ,col = 6}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 43 } },
            { new GameGrid() { row= 8 ,col = 5}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 44 } },
            { new GameGrid() { row= 8, col = 4}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 45 } },
            { new GameGrid() { row= 8,col =  3}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 46 } },
            { new GameGrid() { row= 8,col =  2}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 47 } },
            { new GameGrid() { row= 8,col =  1}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 48 } },
            { new GameGrid() { row= 8,col =  0}, new GameIndex() { posType = Constants.NORMAL_POS,pos = 49 } },
            { new GameGrid() { row= 7,col =  0 }, new GameIndex() { posType = Constants.NORMAL_POS,pos = 50 } },
            { new GameGrid() { row= 6,col =  0 }, new GameIndex() { posType = Constants.NORMAL_POS,pos = 51 } },
        };
        foreach (KeyValuePair<GameGrid, GameIndex> pair in temp)
        {
            gridToIndex[pair.Key] = pair.Value;
            indexToGrid[pair.Value] = pair.Key;
        }
        GameIndex outresult;
        if (temp.TryGetValue(grid, out outresult))
        {
            return outresult;
        }

        switch (players[currentPlayer])
        {
            case Player.RED:
                temp = new Dictionary<GameGrid, GameIndex>()
                {
                    { new GameGrid() { row= 7,col = 1}, new GameIndex() { posType = Constants.END_POS,pos = 0 } },
                    { new GameGrid() { row= 7,col = 2}, new GameIndex() { posType = Constants.END_POS,pos = 1 } },
                    { new GameGrid() { row= 7,col = 3}, new GameIndex() { posType = Constants.END_POS,pos = 2 } },
                    { new GameGrid() { row= 7,col = 4}, new GameIndex() { posType = Constants.END_POS,pos = 3 } },
                    { new GameGrid() { row= 7,col = 5}, new GameIndex() { posType = Constants.END_POS,pos = 4 } },
                    { new GameGrid() { row= 7,col = 6}, new GameIndex() { posType = Constants.END_POS,pos = 5 } },
                    { new GameGrid() { row= 1,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 1,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 1,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 1,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } }
                };
                indexToGridEnd[Player.RED] = new Dictionary<GameIndex, GameGrid>();
                foreach (KeyValuePair<GameGrid, GameIndex> pair in temp)
                {
                    gridToIndex[pair.Key] = pair.Value;
                    indexToGridEnd[Player.RED][pair.Value] = pair.Key;
                }
                if (temp.TryGetValue(grid, out outresult))
                {
                    return outresult;
                }
                break;

            case Player.BLUE:
                temp = new Dictionary<GameGrid, GameIndex>()
                {
                    { new GameGrid() { row= 1,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 1 } },
                    { new GameGrid() { row= 3,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 2 } },
                    { new GameGrid() { row= 4,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 3 } },
                    { new GameGrid() { row= 5,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 4 } },
                    { new GameGrid() { row= 6,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 5 } },
                    { new GameGrid() { row= 1,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 1,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 1,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 1,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 2,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 3,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 4,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                };
                indexToGridEnd[Player.BLUE] = new Dictionary<GameIndex, GameGrid>();
                foreach (KeyValuePair<GameGrid, GameIndex> pair in temp)
                {
                    gridToIndex[pair.Key] = pair.Value;
                    indexToGridEnd[Player.BLUE][pair.Value] = pair.Key;
                }
                if (temp.TryGetValue(grid, out outresult))
                {
                    return outresult;
                }
                break;

            case Player.YELLOW:
                temp = new Dictionary<GameGrid, GameIndex>()
                {
                    { new GameGrid() { row= 7,col = 13}, new GameIndex() { posType = Constants.END_POS,pos = 0 } },
                    { new GameGrid() { row= 7,col = 12}, new GameIndex() { posType = Constants.END_POS,pos = 1 } },
                    { new GameGrid() { row= 7,col = 11}, new GameIndex() { posType = Constants.END_POS,pos = 2 } },
                    { new GameGrid() { row= 7,col = 10}, new GameIndex() { posType = Constants.END_POS,pos = 3 } },
                    { new GameGrid() { row= 7,col = 9}, new GameIndex() { posType = Constants.END_POS,pos = 4 } },
                    { new GameGrid() { row= 7,col = 8}, new GameIndex() { posType = Constants.END_POS,pos = 5 } },
                    { new GameGrid() { row= 10,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 10,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 10,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 10,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 10}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 11}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 12}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 13}, new GameIndex() { posType = Constants.START_POS,pos = 0 } }
                };
                indexToGridEnd[Player.YELLOW] = new Dictionary<GameIndex, GameGrid>();
                foreach (KeyValuePair<GameGrid, GameIndex> pair in temp)
                {
                    gridToIndex[pair.Key] = pair.Value;
                    indexToGridEnd[Player.YELLOW][pair.Value] = pair.Key;
                }
                if (temp.TryGetValue(grid, out outresult))
                {
                    return outresult;
                }
                break;

            case Player.GREEN:
                temp = new Dictionary<GameGrid, GameIndex>()
                {
                    { new GameGrid() { row= 13,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 1 } },
                    { new GameGrid() { row= 11,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 2 } },
                    { new GameGrid() { row= 10,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 3 } },
                    { new GameGrid() { row= 9,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 4 } },
                    { new GameGrid() { row= 8,col = 7}, new GameIndex() { posType = Constants.END_POS,pos = 5 } },
                    { new GameGrid() { row= 10,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 10,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 10,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 10,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 11,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 12,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 1}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 2}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 3}, new GameIndex() { posType = Constants.START_POS,pos = 0 } },
                    { new GameGrid() { row= 13,col = 4}, new GameIndex() { posType = Constants.START_POS,pos = 0 } }
                };
                indexToGridEnd[Player.GREEN] = new Dictionary<GameIndex, GameGrid>();
                foreach (KeyValuePair<GameGrid, GameIndex> pair in temp)
                {
                    gridToIndex[pair.Key] = pair.Value;
                    indexToGridEnd[Player.GREEN][pair.Value] = pair.Key;
                }
                if (temp.TryGetValue(grid, out outresult))
                {
                    return outresult;
                }
                break;
        }
        return new GameIndex() { pos = -1 };
    }

    GameGrid TransformToGrid(Vector3 temp)
    {
        return new GameGrid() { row = -(int)temp.y, col = (int)(temp.x) };
    }

    Vector3 GridToTransform(GameGrid temp)
    {
        return new Vector3() { x = temp.col + 0.5f, y = -(temp.row + 0.5f) ,z  = -1};
    }
}
