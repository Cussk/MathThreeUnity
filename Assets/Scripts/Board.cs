using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UnityEngine;

public class Board : MonoBehaviour
{
   [SerializeField] private int width;
   [SerializeField] private int height;
   [SerializeField] private int borderSize;

    [SerializeField] float swapTime = 0.3f;

   [SerializeField] private GameObject tilePrefab;
   [SerializeField] private GameObject[] gamePiecePrefabs;

    //2D arrays
    private Tile[,] m_allTiles;
    private GamePiece[,] m_allGamePieces;

    private Tile m_clickedTile;
    private Tile m_targetTile;

    private bool m_SwitchingEnabled = true;

    void Start()
    {
        m_allTiles= new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];

        SetUpTiles();
        SetupCamera();
        FillBoardRandomPieces(10, 0.5f);
        //HighlightMatches();
    }

    void SetUpTiles()
    {
        for (int i =0; i < width; i++)
        {
            for (int j =0; j < height; j++)
            {
               
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity);

                tile.name = "Tile (" + i + "," + j + ")";

                //add tiles to 2D array
                m_allTiles[i, j] = tile.GetComponent<Tile>();
                
                //parent tiles to board, move tiles with board
                tile.transform.parent = transform;

                //gives Tile object x,y coordinates on the game board
                m_allTiles[i, j].Initialize(i, j, this);
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
                if (m_allGamePieces[i, j] == null)
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
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);

        if (randomPiece != null)
        {
            randomPiece.GetComponent<GamePiece>().Initialize(this);

            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), xCoordinate, yCoordinate);

            if (falseYOffset != 0)
            {
                //start new pieces above board
                randomPiece.transform.position = new Vector3(xCoordinate, (yCoordinate + falseYOffset), 0);
                randomPiece.GetComponent<GamePiece>().MovePiece(xCoordinate, yCoordinate, moveTime);
            }

            //child pieces transform to board
            randomPiece.transform.parent = transform;

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
        if (m_SwitchingEnabled)
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

                //if no matches founds swap tiles back to original positions
                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                {
                    clickedPiece.MovePiece(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.MovePiece(targetTile.xIndex, targetTile.yIndex, swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    ClearAndRefiilBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());

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
        SpriteRenderer spriteRenderer = m_allTiles[xCoordinate, yCoordinate].GetComponent<SpriteRenderer>();

        //set alpha to 0
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    void HighlightTileOn(int xCoordinate, int yCoordinate, Color color)
    {
        SpriteRenderer spriteRenderer = m_allTiles[xCoordinate, yCoordinate].GetComponent<SpriteRenderer>();
        
        spriteRenderer.color = color;
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

        HighlightTileOff(xCoordinate, yCoordinate);
    }

    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach(GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
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
            //if a piece in column is null
            if (m_allGamePieces[column, i] == null)
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
        m_SwitchingEnabled = false;

        List<GamePiece> matches = gamePieces;

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));

            yield return StartCoroutine(RefillRoutine());
        }
        while (matches.Count != 0);

        m_SwitchingEnabled = true;
    }

    IEnumerator RefillRoutine()
    {
        FillBoardRandomPieces(10, 0.5f);

        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.5f);

        bool isFinished = false;

        //loop though new matches found as columns collapse
        while (!isFinished)
        {
            ClearPieceAt(gamePieces);

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
}