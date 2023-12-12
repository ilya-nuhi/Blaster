using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum PieceType
{
	Red,
	Green,
	Blue,
	Yellow,
	TNT,
	Box,
	Stone,
	Vase
}

public class GamePiece : MonoBehaviour
{
    public int xIndex;
	public int yIndex;
	Board m_board;
    public PieceType pieceType;
    public bool isBreakableWithMatch = false;
    public bool isMatchingPiece = false;
	public bool isBreakable = false;
	public bool isMovable = true;
	public int breakableValue = 0;
	public Sprite[] breakableSprites;
	public Sprite tntStateSprite;
	public Sprite normalStateSprite;
	bool m_isMoving = false;
	public InterpType interpolation = InterpType.SmootherStep;
	SpriteRenderer m_spriteRenderer;
	public enum InterpType
	{
		Linear,
		EaseOut,
		EaseIn,
		SmoothStep,
		SmootherStep
	};

	void Awake () 
	{
		m_spriteRenderer = GetComponent<SpriteRenderer>();
	}

    public void Init(Board board)
	{
		this.m_board = board;
	}

	public void SetCoord(int x, int y)
	{
		xIndex = x;
		yIndex = y;
	}

	public void Move (int destX, int destY, float timeToMove)
	{

		if (!m_isMoving)
		{
			StartCoroutine(MoveRoutine(new Vector3(destX, destY,0), timeToMove));	
		}
	}

	IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
	{
		Vector3 startPosition = transform.position;

		bool reachedDestination = false;

		float elapsedTime = 0f;

		m_isMoving = true;

		while (!reachedDestination)
		{
			// if we are close enough to destination
			if (Vector3.Distance(transform.position, destination) < 0.01f)
			{

				reachedDestination = true;

				if (m_board !=null)
				{
					m_board.PlaceGamePiece(this, (int) destination.x, (int) destination.y);

				}

				break;
			}

			// track the total running time
			elapsedTime += Time.deltaTime;

			// calculate the Lerp value
			float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

			switch (interpolation)
			{
				case InterpType.Linear:
					break;
				case InterpType.EaseOut:
					t = Mathf.Sin(t * Mathf.PI * 0.5f);
					break;
				case InterpType.EaseIn:
					t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
					break;
				case InterpType.SmoothStep:
					t = t*t*(3 - 2*t);
					break;
				case InterpType.SmootherStep:
					t =  t*t*t*(t*(t*6 - 15) + 10);
					break;
			}

			// move the game piece
			transform.position = Vector3.Lerp(startPosition, destination, t);

			// wait until next frame
			yield return null;
		}

		m_isMoving = false;

	}

	public void BreakGamePiece(){
		breakableValue = Mathf.Clamp(--breakableValue,0,breakableValue);

		if(breakableValue>=0){
			m_spriteRenderer.sprite = breakableSprites[breakableValue];
		}
	}
}
