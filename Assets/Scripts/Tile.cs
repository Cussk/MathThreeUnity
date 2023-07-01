using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private int xIndex;
    [SerializeField] private int yIndex;

    private Board m_board;

    void Start()
    {
        
    }

    public void Initialize(int xCoordinate, int yCoordinate, Board board)
    {
        xIndex = xCoordinate;
        yIndex = yCoordinate;
        m_board = board;
    }
}
