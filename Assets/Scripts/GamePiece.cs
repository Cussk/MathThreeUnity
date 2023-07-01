using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public MatchValue matchValue;

    public int xIndex;
    public int yIndex;

    [SerializeField] private InterpolationType interpolation = InterpolationType.SmootherStep;

    private Board m_board;

    private bool m_isMoving = false;

    public enum InterpolationType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    }

    public enum MatchValue
    {
        Yellow,
        Blue,
        Pink,
        Purple,
        Green,
        Teal,
        Red,
        Cyan,
        Wild

    }

    void Start()
    {
        
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MovePiece((int)transform.position.x + 2, (int)transform.position.y, 0.5f);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MovePiece((int)transform.position.x - 2, (int)transform.position.y, 0.5f);
        }*/
    }

    public void Initialize(Board board)
    {
        m_board = board;
    }

    public void SetCoordinates(int xCoordinate, int yCoordinate)
    {
        xIndex = xCoordinate;
        yIndex = yCoordinate;
    }

    public void MovePiece(int destinationXCoordinate, int destinationYCoordinate, float timeToMove)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destinationXCoordinate, destinationYCoordinate, 0), timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;

        bool reachedDestination = false;

        float elapsedTime = 0.0f;

        m_isMoving = true;

        while (!reachedDestination)
        {
            //if close enough to destination
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;

                if (m_board != null)
                {
                    //set pieces new position and update coordinates
                    m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }

                break;
            }

            elapsedTime += Time.deltaTime;

            float lerpValue = Mathf.Clamp((elapsedTime / timeToMove), 0.0f, 1.0f);

            //interpolation curve options
            switch (interpolation)
            {
                case InterpolationType.Linear:
                    break;
                case InterpolationType.EaseOut:
                    lerpValue = Mathf.Sin(lerpValue * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.EaseIn:
                    lerpValue = 1 - Mathf.Cos(lerpValue * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.SmoothStep:
                    lerpValue = lerpValue * lerpValue * (3 - 2 * lerpValue);
                    break;
                case InterpolationType.SmootherStep:
                    lerpValue = lerpValue * lerpValue * lerpValue * (lerpValue * (lerpValue * 6 - 15) + 10);
                    break;


            }
                         
            //move game piece
            transform.position = Vector3.Lerp(startPosition, destination, lerpValue);

            yield return null;
        }

        m_isMoving = false;
    }
}
