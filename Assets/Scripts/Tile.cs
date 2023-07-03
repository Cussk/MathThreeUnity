using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public Color normalTileColor;

    public int breakableValue = 0;
    public Sprite[] breakableSprites; 

    public TileType tileType = TileType.Normal;

    private Board m_board;

    private SpriteRenderer m_spriteRenderer;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
    }

    public void Initialize(int xCoordinate, int yCoordinate, Board board)
    {
        xIndex = xCoordinate;
        yIndex = yCoordinate;
        m_board = board;

        if (tileType == TileType.Breakable)
        {
            if (breakableSprites[breakableValue] != null)
            {
                //set sprite renderer to sprite designated at the breakableValue
                m_spriteRenderer.sprite = breakableSprites[breakableValue];
            }
        }
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

    public void BreakTile()
    {
        if (tileType != TileType.Breakable) 
        { 
            return; 
        }

        StartCoroutine(BreakTileRoutine());
    }

    IEnumerator BreakTileRoutine()
    {
        breakableValue = Mathf.Clamp(--breakableValue, 0, breakableValue);

        yield return new WaitForSeconds(0.25f);

        if (breakableSprites[breakableValue] != null)
        {
            //set sprite renderer to sprite designated at the breakableValue
            m_spriteRenderer.sprite = breakableSprites[breakableValue];
        }

        //when breable value reaches 0 turn into normal tile
        if (breakableValue <= 0)
        {
            tileType = TileType.Normal;
            m_spriteRenderer.color = normalTileColor;
        }
    }
}
