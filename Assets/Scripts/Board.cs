using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    void Start()
    {
        m_allTiles= new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];

        SetUpTiles();
        SetupCamera();
        FillBoardRandomPieces();
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

    void FillBoardRandomPieces()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);

                if (randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Initialize(this);

                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);

                    //child pieces transform to board
                    randomPiece.transform.parent = transform;
                }
            }
        }
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

            yield return new WaitForSeconds(swapTime);

            //highlight matching tiles
            HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);
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

    private void HighlightMatchesAt(int xCoordinate, int yCoordinate)
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

    private List<GamePiece> FindMatchesAt(int xCoordinate, int yCoordinate, int minimumLength = 3)
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
}