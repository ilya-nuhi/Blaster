using System.Collections;
using System.Collections.Generic;
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
    public void Init(Board board)
	{
		this.m_board = board;
	}

	public void SetCoord(int x, int y)
	{
		xIndex = x;
		yIndex = y;
	}
}
