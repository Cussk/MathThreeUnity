using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;

    [SerializeField] float swapTime = 0.3f;

    [SerializeField] int fillYOffset = 10;
    [SerializeField] float fillMoveTime = 0.5f;

    [SerializeField] private GameObject tileNormalPrefab;
    [SerializeField] private GameObject tileObstaclePrefab;

    [SerializeField] private GameObject adjacentBombPrefab;
    [SerializeField] private GameObject columnBombPrefab;
    [SerializeField] private GameObject rowBombPrefab;
    [SerializeField] private GameObject colorBombPrefab;

    [SerializeField] private GameObject[] gamePiecePrefabs;

    //2D arrays
    private Tile[,] m_allTiles;
    private GamePiece[,] m_allGamePieces;

    private Tile m_clickedTile;
    private Tile m_targetTile;

    private GameObject m_clickedTileBomb;
    private GameObject m_targetTileBomb;

    private ParticleManager m_particleManager;

    private bool m_switchingEnabled = true;

    [SerializeField] private StartingObject[] startingTiles;
    [SerializeField] private StartingObject[] startingGamePieces;

    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    void Start()
    {
        m_allTiles= new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];

        SetupTiles();
        SetupPieces();

        SetupCamera();
        FillBoardRandomPieces(fillYOffset, fillMoveTime);
        m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();

        //HighlightMatches();
    }

    void MakeTile(GameObject prefab, int xCoordinate, int yCoordinate, int zCoordinate = 0)
    {
        if (prefab != null && IsWithinBounds(xCoordinate, yCoordinate))
        {
            GameObject tile = Instantiate(prefab, new Vector3(xCoordinate, yCoordinate, zCoordinate), Quaternion.identity);

            tile.name = "Tile (" + xCoordinate + "," + yCoordinate + ")";

            //add tiles to 2D array
            m_allTiles[xCoordinate, yCoordinate] = tile.GetComponent<Tile>();

            //parent tiles to board, move tiles with board
            tile.transform.parent = transform;

            //gives Tile object x,y coordinates on the game board
            m_allTiles[xCoordinate, yCoordinate].Initialize(xCoordinate, yCoordinate, this);
        }
    }

    void MakeGamePiece (GameObject prefab, int xCoordinate, int yCoordinate, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (prefab != null && IsWithinBounds(xCoordinate, yCoordinate))
        {
            prefab.GetComponent<GamePiece>().Initialize(this);

            PlaceGamePiece(prefab.GetComponent<GamePiece>(), xCoordinate, yCoordinate);

            if (falseYOffset != 0)
            {
                //start new pieces above board
                prefab.transform.position = new Vector3(xCoordinate, (yCoordinate + falseYOffset), 0);
                prefab.GetComponent<GamePiece>().MovePiece(xCoordinate, yCoordinate, moveTime);
            }

            //child pieces transform to board
            prefab.transform.parent = transform;
        }
    }

    GameObject MakeBomb(GameObject prefab, int xCoordinate, int yCoordinate)
    {
        if (prefab != null && IsWithinBounds(xCoordinate, yCoordinate))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(xCoordinate, yCoordinate, 0), Quaternion.identity);

            //gives bomb x,y coordinates on the game board
            bomb.GetComponent<Bomb>().Initialize(this);
            //set coordinates in editor
            bomb.GetComponent<Bomb>().SetCoordinates(xCoordinate, yCoordinate);
            //child bomb to board
            bomb.transform.parent = transform;

            return bomb;
        }
        return null;
    }

    void SetupTiles()
    {
        foreach (StartingObject sTile in startingTiles)
        {
            if (sTile != null)
            {
                MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i, j] == null)
                {
                    MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

    void SetupPieces()
    {
        foreach (StartingObject sGamePiece in startingGamePieces)
        {
            if (sGamePiece != null)
            {
                GameObject gamePiece = Instantiate(sGamePiece.prefab, new Vector3(sGamePiece.x, sGamePiece.y, 0), Quaternion.identity);

                MakeGamePiece(gamePiece, sGamePiece.x, sGamePiece.y, fillYOffset, fillMoveTime);
            }
        }
    }

    void SetupCamera()
    {
        //camera starting position
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2.0f, (float)(height -1) / 2.0f, -10.0f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = (float)height / 2.0f + (float)borderSize;
        
        float horizontalSize = ((float)width / 2.0f + (float)borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    GameObject GetRandomGamePiece()
    {
        int randomIndex = Random.Range(0, gamePiecePrefabs.Length);

        if (gamePiecePrefabs[randomIndex] == null)
        {
            Debug.LogWarning("BOARD: " + randomIndex + "does not contain a valid GamePiece prefab.");
        }

        return gamePiecePrefabs[randomIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int xCoordinate, int yCoordinate)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid Game Piece.");

            return;
        }

        gamePiece.transform.position = new Vector3 (xCoordinate, yCoordinate, 0);
        gamePiece.transform.rotation = Quaternion.identity;

        if (IsWithinBounds(xCoordinate, yCoordinate))
        {
            //add gamePieces to 2D array
            m_allGamePieces[xCoordinate, yCoordinate] = gamePiece;
        }
        
        gamePiece.SetCoordinates(xCoordinate, yCoordinate);
    }

    bool IsWithinBounds(int xCoordinate, int yCoordinate)
    {
        return (xCoordinate >= 0 && xCoordinate < width && yCoordinate >= 0 && yCoordinate < height);
    }

    void FillBoardRandomPieces(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;
        int iterations = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);
                    iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            Debug.Log("break: Too many consecutive matches");
                            break;
                        }
                    }
                }
            }
        }
    }

    GamePiece FillRandomAt(int xCoordinate, int yCoordinate, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (IsWithinBounds(xCoordinate, yCoordinate))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);

            MakeGamePiece(randomPiece, xCoordinate, yCoordinate, falseYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }
        
        return null;
    }

    bool HasMatchOnFill(int xCoordinate, int yCoordinate, int minimumLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(xCoordinate, yCoordinate, new Vector2(-1, 0), minimumLength);
        List<GamePiece> downwardMatches = FindMatches(xCoordinate, yCoordinate, new Vector2(0, -1), minimumLength);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    public void ClickedTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
            //Debug.Log("clicked tile: " + tile.name);
        }
    }

    public void DragTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }

    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_switchingEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                //switch clickedPiece qnd targetPieces positions
                clickedPiece.MovePiece(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.MovePiece(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> colorMatches = new List<GamePiece>();

                //if swapping player clicked color bomb with gamePiece
                if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
                {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                }
                //if swapping gamePiece that player clicked with color bomb
                else if (IsColorBomb(targetPiece) && !IsColorBomb(clickedPiece))
                {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                //if switched tiles are both color bombs
                else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    //find all gamepieces on board
                    foreach (GamePiece piece in m_allGamePieces)
                    {
                        if (!colorMatches.Contains(piece))
                        {
                            //add all pieces to colorMatches list
                            colorMatches.Add(piece);
                        }
                    }
                }

                //if no matches founds swap tiles back to original positions
                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.MovePiece(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.MovePiece(targetTile.xIndex, targetTile.yIndex, swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    Vector2 swapDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);

                    m_clickedTileBomb = SpawnBomb(clickedTile.xIndex, clickedTile.yIndex, swapDirection, clickedPieceMatches);
                    m_targetTileBomb = SpawnBomb(targetTile.xIndex, targetTile.yIndex, swapDirection, targetPieceMatches);

                    //bomb created from game piece player clicked
                    if (m_clickedTileBomb != null && targetPiece != null)
                    {
                        
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();

                        if (!IsColorBomb(clickedBombPiece))
                        {
                            //make spawned bomb the same color as game pieces matched
                            clickedBombPiece.ChangeColor(targetPiece);
                        }
                    }
                    //bomb created from game piece that was switched with player clicked piece
                    if (m_targetTileBomb != null && clickedPiece != null)
                    {
                        
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();

                        if (!IsColorBomb(targetBombPiece))
                        {
                            //make spawned bomb the same color as game pieces matched
                            targetBombPiece.ChangeColor(clickedPiece);
                        }
                    }

                    //combine clicked matches, target mataches, and color bomb matches
                    ClearAndRefiilBoard(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList());

                    //ClearPieceAt(clickedPieceMatches);
                    //ClearPieceAt(targetPieceMatches);

                    //CollapseColumn(clickedPieceMatches);
                    //CollapseColumn(targetPieceMatches);

                    //highlight matching tiles debug
                    //HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                    //HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);
                }
            }
        }
    }

    bool IsNextTo(Tile start, Tile end)
    {
        //checks if surrounding tiles are within 1 space left, right, up, down
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;

    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minimumLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            //check spaces in vertical line from start piece
            nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1 ) * i;
            //check spaces in horizontal line from start piece
            nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1 ) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                //if pieces color matches and not alredy in list, add to list
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= minimumLength)
        {
            return matches;
        }

        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minimumLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        //combines lists
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minimumLength) ? combinedMatches : null;
    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minimumLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        //combines lists
        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minimumLength) ? combinedMatches : null;
    }

    void HighlightTileOff(int xCoordinate, int yCoordinate)
    {
        if (m_allTiles[xCoordinate, yCoordinate].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[xCoordinate, yCoordinate].GetComponent<SpriteRenderer>();

            //set alpha to 0
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int xCoordinate, int yCoordinate, Color color)
    {
        if (m_allTiles[xCoordinate, yCoordinate].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[xCoordinate, yCoordinate].GetComponent<SpriteRenderer>();

            spriteRenderer.color = color;
        }
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighlightMatchesAt(int xCoordinate, int yCoordinate)
    {
        HighlightTileOff(xCoordinate, yCoordinate);

        List<GamePiece> combinedMatches = FindMatchesAt(xCoordinate, yCoordinate);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                //set tile color to same color as game pieces that match
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                //set tile color to same color as game pieces that match
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    List<GamePiece> FindMatchesAt(int xCoordinate, int yCoordinate, int minimumLength = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(xCoordinate, yCoordinate, minimumLength);
        List<GamePiece> verticalMatches = FindVerticalMatches(xCoordinate, yCoordinate, minimumLength);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<GamePiece>();
        }

        if (verticalMatches == null)
        {
            verticalMatches = new List<GamePiece>();
        }

        //combines lists
        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }

    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minimumLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            //combine lists of found matches for each piece in lisr
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minimumLength)).ToList();
        }

        return matches;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);

                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    void ClearPieceAt(int xCoordinate, int yCoordinate)
    {
        GamePiece pieceToClear = m_allGamePieces[xCoordinate, yCoordinate];

        if (pieceToClear != null)
        {
            m_allGamePieces[xCoordinate, yCoordinate] = null;
            Destroy(pieceToClear.gameObject);
        }

        //HighlightTileOff(xCoordinate, yCoordinate);
    }

    void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach(GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);

                if (m_particleManager != null)
                {
                    if (bombedPieces.Contains(piece))
                    {
                        //play particle effect on each piece cleared by a bomb
                        m_particleManager.BombFXAt(piece.xIndex, piece.yIndex);
                    }
                    else
                    {
                        //play particle effect on each piece cleared
                        m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                    }
                }
            }
        }
    }

    void BreakTileAt(int xCoordinate, int yCoordinate)
    {
        //check all tiles for breakable tiles
        Tile tileToBreak = m_allTiles[xCoordinate,yCoordinate];
        
        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if (m_particleManager != null)
            {
                //play different particle effect depending on breakableValue level
                m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, xCoordinate, yCoordinate);
            }

            tileToBreak.BreakTile();
        }
    }

    void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        //loop through pieces in a column
        for (int i = 0; i < height; i++)
        {
            //if a piece in column is null and not an obstacle tile
            if (m_allGamePieces[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle)
            {
                //loop through spaces in column starting 1 place above missing piece
                for (int j = i + 1; j < height; j++)
                {
                    //if piece at this position is not also null
                    if (m_allGamePieces[column, j] != null)
                    {
                        //move piece in column down to empty space
                        m_allGamePieces[column, j].MovePiece(column, i, collapseTime * (j - i));

                        //force transform and coordinate set, not waiting for MovePiece function
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoordinates(column, i);

                        //add piece that moved to list in new position if not in list already
                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }

                        //set space where piece moved from to null
                        m_allGamePieces[column, j] = null;

                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columsToCollapse = GetColumns(gamePieces);

        foreach (int column in columsToCollapse)
        {
            //combine lists of movingPieces for each column in the columns list
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if (!columns.Contains(piece.xIndex))
            {
                columns.Add(piece.xIndex);
            }
        }

        return columns;
    }

    void ClearAndRefiilBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        m_switchingEnabled = false;

        List<GamePiece> matches = gamePieces;

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.2f);
        }
        while (matches.Count != 0);

        m_switchingEnabled = true;
    }

    IEnumerator RefillRoutine()
    {
        FillBoardRandomPieces(fillYOffset, fillMoveTime);

        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.2f);

        bool isFinished = false;

        //loop though new matches found as columns collapse
        while (!isFinished)
        {
            //get the list of pieces affected by bombs
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            //combine with master list of pieces to be cleared
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            //get multiple instances of bombs for chaining
            bombedPieces = GetBombedPieces(gamePieces);
            //combine with master list again
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            ClearPieceAt(gamePieces, bombedPieces);
            BreakTileAt(gamePieces);

            if (m_clickedTileBomb != null)
            {
                //explode bomb and make null
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }
            if (m_targetTileBomb != null)
            {
                //explode bomb and make null
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }


            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                //recursively start ClearAndCollapseRoutine again on all new matches
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            { 
                //if the piece has not reached its destination return false
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            //if there is a gamepiece in row
            if (m_allGamePieces[i, row] != null)
            {
                //add gamepiece to list
                gamePieces.Add(m_allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }
    List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            //if there is a gamepiece in column
            if (m_allGamePieces[column, i] != null)
            {
                //add gamepiece to list
                gamePieces.Add(m_allGamePieces[column, i]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces(int xCoordinate, int yCoordinate, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        //get pieces in x corrdinate range
        for (int i = xCoordinate - offset; i <= xCoordinate + offset; i++) 
        {
            //get pieces in y corrdinate range
            for (int j = yCoordinate - offset; j <= yCoordinate + offset; j++)
            {
                //if pieces are still within the board
                if (IsWithinBounds(i, j))
                {
                    //add game pieces to list
                    gamePieces.Add(m_allGamePieces[i, j]);
                }
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        //all pieces, potentially multiple bombs
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                //single bomb list
                List<GamePiece> piecesToClear = new List<GamePiece>();

                Bomb bomb = piece.GetComponent<Bomb>();

                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            break;


                    }
                    //combine lists
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                }
            }
        }
        return allPiecesToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                //if x or y start off the board, set to first piece on board
                if (xStart == -1 || yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }
                //if matching piece on row and no match on column
                if (piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }
                //if match on column and no match on row
                if (piece.xIndex == xStart && piece.yIndex != yStart) 
                {
                    vertical = true;
                }
            }
        }
        return (horizontal && vertical);
    }

    GameObject SpawnBomb(int xCoordinate, int yCoordinate, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;

        if (gamePieces.Count >= 4) 
        {
            //adjacent bomb
            if (IsCornerMatch(gamePieces))
            {
                if (adjacentBombPrefab != null)
                {
                    bomb = MakeBomb(adjacentBombPrefab, xCoordinate, yCoordinate);
                }
            }
            else
            {
                if (gamePieces.Count >= 5)
                {
                    bomb = MakeBomb(colorBombPrefab, xCoordinate, yCoordinate);
                }
                else
                {
                    //row bomb
                    if (swapDirection.x != 0)
                    {
                        if (rowBombPrefab != null)
                        {
                            bomb = MakeBomb(rowBombPrefab, xCoordinate, yCoordinate);
                        }
                    }
                    //column bomb
                    else
                    {
                        bomb = MakeBomb(columnBombPrefab, xCoordinate, yCoordinate);
                    }
                }
            }
        }
        return bomb;
    }

    void ActivateBomb(GameObject bomb)
    {
        int xCoordinate = (int)bomb.transform.position.x;
        int yCoordinate = (int)bomb.transform.position.y;
            
        if (IsWithinBounds(xCoordinate, yCoordinate))
        {
            //add bombs to array to activate their explosions
            m_allGamePieces[xCoordinate, yCoordinate] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue(MatchValue matchValue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if game piece has same value as desired value add to list
                if (m_allGamePieces[i, j].matchValue == matchValue)
                {
                    foundPieces.Add(m_allGamePieces[i, j]);
                }
            }
        }
        return foundPieces;
    }

    bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if (bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }
        return false;
    }
 }