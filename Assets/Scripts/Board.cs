using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
   [SerializeField] private int width;
   [SerializeField] private int height;
   [SerializeField] private int borderSize;

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] gamePiecePrefabs;

    //2D arrays
    private Tile[,] m_allTiles;
    private GamePiece[,] m_allGamePieces;

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

    void PlaceGamePiece(GamePiece gamePiece, int xCoordinate, int yCoordinate)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid Game Piece.");

            return;
        }

        gamePiece.transform.position = new Vector3 (xCoordinate, yCoordinate, 0);
        gamePiece.transform.rotation = Quaternion.identity;

        gamePiece.SetCoordinates(xCoordinate, yCoordinate);
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
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                }
            }
        }
    }
}