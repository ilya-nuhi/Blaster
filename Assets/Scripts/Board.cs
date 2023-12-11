using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Random = UnityEngine.Random;

public class LevelInfo
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
        
    }

public class Board : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] GameObject tileNormalPrefab;
    [SerializeField] GameObject[] gamePiecePrefabs;

    [TextArea(3,5)]
    [SerializeField] string[] levelJsonPath;
    [SerializeField ]int level=1;
    
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;
    LevelInfo currentLevel;

    Tile m_clickedTile;


    void Start()
    {
        currentLevel = LoadJson(level);
        InitLevel(currentLevel);
        
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];

        
        SetupTiles();
        SetupGamePieces();
        SetupCamera();
    }

    private LevelInfo LoadJson(int level)
    {
        try
        {
            using (StreamReader r = new StreamReader(levelJsonPath[level-1]))
            {
                string json = r.ReadToEnd();
                LevelInfo currentLevel = JsonConvert.DeserializeObject<LevelInfo>(json);
                return currentLevel;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    private void InitLevel(LevelInfo currentLevel)
    {
        width = currentLevel.grid_width;
        height = currentLevel.grid_height;
    }

    void SetupTiles()
    {
        for (int i = 0; i < width ; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i, j] == null)
                {
                    MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

    void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";
            m_allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            m_allTiles[x, y].Init(x, y, this);
        }
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height*1.3f - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = (float)height * 1.6f / 2f;

        float horizontalSize = ((float)width * 1.3f / 2f ) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

    }

    void SetupGamePieces()
    {
        
        if(currentLevel!=null){
            int index = 0;
            for(int y = 0; y < currentLevel.grid_height;y++){
                for(int x = 0; x < currentLevel.grid_width; x++){
                    if(m_allGamePieces[x, y] == null){
                        if(currentLevel.grid[index]!=null)
                            PlaceGamePiece(currentLevel.grid[index], x, y);
                            index++;
                    }
                }
            }
        }
    }

    void PlaceGamePiece(string item, int x, int y)
    {
        GameObject piece = null;
        switch(item){
            case "r":
                piece = Instantiate(gamePiecePrefabs[0], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "g":
                piece = Instantiate(gamePiecePrefabs[1], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "b":
                piece = Instantiate(gamePiecePrefabs[2], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "y":
                piece = Instantiate(gamePiecePrefabs[3], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "rand":
                int randomPiece = Random.Range(0,4);
                piece = Instantiate(gamePiecePrefabs[randomPiece], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "t":
                piece = Instantiate(gamePiecePrefabs[4], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "bo":
                piece = Instantiate(gamePiecePrefabs[5], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "s":
                piece = Instantiate(gamePiecePrefabs[6], new Vector3(x,y,0), Quaternion.identity);
                break;
            case "v":
                piece = Instantiate(gamePiecePrefabs[7], new Vector3(x,y,0), Quaternion.identity);
                break;
            default:
                Debug.LogWarning("Item name doesn't match.");
                break;
        }

        if(piece!=null){
            GamePiece gamePiece = piece.GetComponent<GamePiece>();
            gamePiece.Init(this);
            gamePiece.SetCoord(x,y);
            m_allGamePieces[x,y] = gamePiece;
        }

    }

    public void ClickTile(Tile tile)
    {
        m_clickedTile = tile;
        GamePiece clickedPiece = m_allGamePieces[tile.xIndex,tile.yIndex];
        if(clickedPiece!=null){
            if(clickedPiece.isMatchingPiece){
                BlastRoutine(clickedPiece);
            }
        }
        
        
    }

    private void BlastRoutine(GamePiece clickedPiece)
    {
        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();
        
        (matchingPieces, breakablePieces) = FindAdjacentMatches(clickedPiece);
        if(matchingPieces.Count<2)return;
        else{
            ClearPieces(matchingPieces);
            //BreakPieces(breakablePieces);"
        }
    }

    /*private (List<GamePiece>, List<GamePiece>) FindAdjacentMatches(GamePiece clickedPiece)
    {
        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();

        Queue<GamePiece> pieceQueue = new Queue<GamePiece>();
        pieceQueue.Enqueue(clickedPiece);
        matchingPieces.Add(clickedPiece);
        while(pieceQueue.Count!=0){
            GamePiece piece = pieceQueue.Dequeue();
            if(IsWithinBounds(piece.xIndex+1,piece.yIndex)){
                GamePiece rightPiece = m_allGamePieces[piece.xIndex+1, piece.yIndex];
                //if the right piece has a matching value, that piece will be added to the matching list.
                if(rightPiece!=null){
                    if(!matchingPieces.Contains(rightPiece) && !breakablePieces.Contains(rightPiece)){
                        if(piece.pieceType==rightPiece.pieceType){
                            pieceQueue.Enqueue(rightPiece);
                            matchingPieces.Add(rightPiece);
                        }
                        else if(rightPiece.isBreakableWithMatch){
                            breakablePieces.Add(rightPiece);
                        }
                    }
                }
                
            }
            if(IsWithinBounds(piece.xIndex-1,piece.yIndex)){
                GamePiece leftPiece = m_allGamePieces[piece.xIndex-1, piece.yIndex];
                //if the left piece has a matching value, that piece will be added to the matching list.
                if(leftPiece!=null){
                    if(!matchingPieces.Contains(leftPiece) && !breakablePieces.Contains(leftPiece)){
                        if(piece.pieceType==leftPiece.pieceType){
                            pieceQueue.Enqueue(leftPiece);
                            matchingPieces.Add(leftPiece);
                        }
                        else if(leftPiece.isBreakableWithMatch){
                            breakablePieces.Add(leftPiece);
                        }
                    }
                }
            }
            if(IsWithinBounds(piece.xIndex,piece.yIndex+1)){
                GamePiece upPiece = m_allGamePieces[piece.xIndex, piece.yIndex+1];
                //if the upper piece has a matching value, that piece will be added to the matching list.
                if(upPiece!=null){
                    if(!matchingPieces.Contains(upPiece) && !breakablePieces.Contains(upPiece)){
                        if(piece.pieceType==upPiece.pieceType){
                            pieceQueue.Enqueue(upPiece);
                            matchingPieces.Add(upPiece);
                        }
                        else if(upPiece.isBreakableWithMatch){
                            breakablePieces.Add(upPiece);
                        }
                    }
                }
            }
            if(IsWithinBounds(piece.xIndex,piece.yIndex-1)){
                GamePiece downPiece = m_allGamePieces[piece.xIndex, piece.yIndex-1];
                //if the below piece has a matching value, that piece will be added to the matching list.
                if(downPiece!=null){
                    if(!matchingPieces.Contains(downPiece) && !breakablePieces.Contains(downPiece)){
                        if(piece.pieceType==downPiece.pieceType){
                            pieceQueue.Enqueue(downPiece);
                            matchingPieces.Add(downPiece);
                        }
                        else if(downPiece.isBreakableWithMatch){
                            breakablePieces.Add(downPiece);
                        }
                    }
                }
                
            }
        }
        return (matchingPieces, breakablePieces);
    }*/

    private (List<GamePiece>, List<GamePiece>) FindAdjacentMatches(GamePiece clickedPiece)
{
    List<GamePiece> matchingPieces = new List<GamePiece>();
    List<GamePiece> breakablePieces = new List<GamePiece>();
    Queue<GamePiece> pieceQueue = new Queue<GamePiece>();

    // Recursive function to find adjacent matching pieces
    void FindMatchesRecursive(GamePiece piece)
    {
        if (piece == null || matchingPieces.Contains(piece) || breakablePieces.Contains(piece))
            return;

        matchingPieces.Add(piece);

        if (piece.isBreakableWithMatch)
            breakablePieces.Add(piece);

        // Check adjacent pieces and call recursively
        void CheckAdjacent(int x, int y)
        {
            if (IsWithinBounds(x, y))
            {
                GamePiece adjacentPiece = m_allGamePieces[x, y];
                if (adjacentPiece != null && !matchingPieces.Contains(adjacentPiece) && !breakablePieces.Contains(adjacentPiece))
                {
                    if (piece.pieceType == adjacentPiece.pieceType)
                    {
                        pieceQueue.Enqueue(adjacentPiece);
                        FindMatchesRecursive(adjacentPiece);
                    }
                    else if (adjacentPiece.isBreakableWithMatch)
                    {
                        breakablePieces.Add(adjacentPiece);
                    }
                }
            }
        }

        CheckAdjacent(piece.xIndex + 1, piece.yIndex); // Right
        CheckAdjacent(piece.xIndex - 1, piece.yIndex); // Left
        CheckAdjacent(piece.xIndex, piece.yIndex + 1); // Up
        CheckAdjacent(piece.xIndex, piece.yIndex - 1); // Down
    }

    pieceQueue.Enqueue(clickedPiece);
    FindMatchesRecursive(clickedPiece);

    return (matchingPieces, breakablePieces);
}
    
    private void ClearPieces(List<GamePiece> matchingPieces)
    {
        foreach(var piece in matchingPieces){
            if(piece != null){
                m_allGamePieces[piece.xIndex,piece.yIndex] = null;
                Destroy(piece.gameObject);
            }
        }
        
    }

    private void BreakPieces(List<GamePiece> breakablePieces)
    {
        // foreach(var piece in breakablePieces){
        //     if(piece != null){
        //         m_allGamePieces[piece.xIndex,piece.yIndex] = null;
        //         Destroy(piece.gameObject);
        //     }
        // }
    }


}
