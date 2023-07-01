using System.Collections;
using System.Collections.Generic;
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
        GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

        //switch clickedPiece qnd targetPieces positions
        clickedPiece.MovePiece(targetTile.xIndex, targetTile.yIndex, swapTime);
        targetPiece.MovePiece(clickedTile.xIndex, clickedTile.yIndex, swapTime);
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
}