using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

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

    private void OnMouseDown()
    {
        if (m_board != null)
        {
            m_board.ClickedTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (m_board != null)
        {
            m_board.DragTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (m_board != null)
        {
            m_board.ReleaseTile();
        }
    }
}
