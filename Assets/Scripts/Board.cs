using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.Experimental.GlobalIllumination;
using System;

public class Board : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] GameObject tileNormalPrefab;
    [SerializeField] GameObject[] gamePiecePrefabs;
    [SerializeField] LevelManager levelManager;
    [SerializeField] float waitClickTime = 0.3f;
    [SerializeField] GameObject gridBGPrefab;
    public event Action onValidClick;
    public bool canMove = true;
    
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;
    LevelInfo currentLevel;
    float clickTimer=0;
    
    void Start()
    {
        currentLevel = levelManager.currentLevel;
        InitLevel(currentLevel);
        
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];

        SetupTiles();
        SetupGamePieces();
        CheckTntState();
        SetupCamera();
    }

    private void Update() {
        clickTimer -= Time.deltaTime;
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
        //setting gridbackground
        float gridPosX = (m_allTiles[width-1,0].transform.position.x + m_allTiles[0,0].transform.position.x)/2;
        float gridPosY = (m_allTiles[0,height-1].transform.position.y + m_allTiles[0,0].transform.position.y)/2;
        GameObject gridBG = Instantiate(gridBGPrefab, new Vector3(gridPosX, gridPosY, 0), Quaternion.identity);
        gridBG.GetComponent<SpriteRenderer>().size = new Vector2(width+0.4f,height+0.4f);
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
            piece.transform.parent = transform;
            GamePiece gamePiece = piece.GetComponent<GamePiece>();
            gamePiece.Init(this);
            gamePiece.SetCoord(x,y);
            m_allGamePieces[x,y] = gamePiece;
        }

    }

    public void ClickTile(Tile tile)
    {
        if(!canMove){return;}
        GamePiece clickedPiece = m_allGamePieces[tile.xIndex,tile.yIndex];
        if(clickedPiece!=null && clickTimer<=0){
            if(clickedPiece.isMatchingPiece){
                BlastRoutine(clickedPiece);
            }
            else if(clickedPiece.pieceType==PieceType.TNT){
                if(onValidClick!=null){
                    onValidClick();
                }
                TntRoutine(clickedPiece);
            }
            clickTimer = waitClickTime;
        }
    }

    private void TntRoutine(GamePiece clickedPiece){
        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();
        (matchingPieces, breakablePieces) = TriggerBomb(clickedPiece);
        List<Tile> emptyTiles = GetTilesOfPieces(matchingPieces);
        emptyTiles = emptyTiles.Union(BreakPieces(breakablePieces)).ToList();
        ClearPieces(matchingPieces);
        CollapseRoutine(emptyTiles);
        CheckTntState();
    }

    private void BlastRoutine(GamePiece clickedPiece)
    {
        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();
        int x = clickedPiece.xIndex;
        int y = clickedPiece.yIndex;
        (matchingPieces, breakablePieces) = FindAdjacentMatches(clickedPiece);
        if(matchingPieces.Count<2)return;
        else{
            // if cliclked piece has a match its a valid move
            if(onValidClick!=null){
                onValidClick();
            }
            List<Tile> emptyTiles = GetTilesOfPieces(matchingPieces);
            emptyTiles = emptyTiles.Union(BreakPieces(breakablePieces)).ToList();
            
            ClearPieces(matchingPieces);
            // if cliclked piece is capable of making tnt
            if(clickedPiece.GetComponent<SpriteRenderer>().sprite == clickedPiece.tntStateSprite){
                MakeTnt(x,y);
            }
            CollapseRoutine(emptyTiles);
            CheckTntState();
        }
    }

    List<Tile> GetTilesOfPieces(List<GamePiece> gamePieces){
        List<Tile> tiles = new List<Tile>();
        foreach(var piece in gamePieces){
            if(!tiles.Contains(m_allTiles[piece.xIndex, piece.yIndex])){
                tiles.Add(m_allTiles[piece.xIndex, piece.yIndex]);
            }
        }
        return tiles;
    }

    private (List<GamePiece>, List<GamePiece>) FindAdjacentMatches(GamePiece clickedPiece)
    {
        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();

        Queue<GamePiece> pieceQueue = new Queue<GamePiece>();
        pieceQueue.Enqueue(clickedPiece);
        matchingPieces.Add(clickedPiece);
        while(pieceQueue.Count!=0){
            GamePiece piece = pieceQueue.Dequeue();
            if(piece.isMatchingPiece){
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
        }
        return (matchingPieces, breakablePieces);
    }

    //Recursive function of FindAdjacentMatches function
    /*private (List<GamePiece>, List<GamePiece>) FindAdjacentMatches(GamePiece clickedPiece)
    {
        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();

        // Recursive function to find adjacent matching pieces
        void FindMatchesRecursive(GamePiece piece)
        {
            if (piece == null || matchingPieces.Contains(piece) || breakablePieces.Contains(piece))
                return;

            matchingPieces.Add(piece);

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

        FindMatchesRecursive(clickedPiece);
        return (matchingPieces, breakablePieces);
    }*/
    
    private void ClearPieces(List<GamePiece> gamePieces)
    {
        foreach(var piece in gamePieces){
            if(piece != null){
                m_allGamePieces[piece.xIndex,piece.yIndex] = null;
                Destroy(piece.gameObject);
            }
        }
    }

    private List<Tile> BreakPieces(List<GamePiece> breakablePieces)
    {
        List<Tile> clearedTiles = new List<Tile>();
        foreach(var piece in breakablePieces){
            if(piece != null){
                if(piece.isBreakable){
                    if(piece.breakableValue>0){
                        piece.BreakGamePiece();
                    }
                    else{
                        clearedTiles.Add(m_allTiles[piece.xIndex,piece.yIndex]);
                        switch(piece.pieceType){
                            case PieceType.Box:
                                levelManager.boxCount--;
                                break;
                            case PieceType.Stone:
                                levelManager.stoneCount--;
                                break;
                            case PieceType.Vase:
                                levelManager.vaseCount--;
                                break;
                            default:
                                Debug.LogWarning("WARNING! Piece is not breakable but trying to break it.");
                                break;
                        }
                        m_allGamePieces[piece.xIndex,piece.yIndex] = null;
                        Destroy(piece.gameObject);
                    }
                }
                
            }
        }
        levelManager.CheckGoals();
        return clearedTiles;
    }

    private void CollapseRoutine(List<Tile> emptyTiles)
    {
        List<int> columns = new List<int>();
        foreach(var tile in emptyTiles){
            if(!columns.Contains(tile.xIndex)){
                columns.Add(tile.xIndex);
                CollapseColumn(tile.xIndex);
                RefillColumn(tile.xIndex);
            }
        }
    }

    private void CollapseColumn(int column, float collapseTime = 0.1f)
    {
        for (int i = 0; i < height-1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {

                
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null && m_allGamePieces[column, j].isMovable)
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));

                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
    }

    // public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    // {
    //     if (gamePiece == null)
    //     {
    //         Debug.LogWarning("BOARD:  Invalid GamePiece!");
    //         return;
    //     }

    //     gamePiece.transform.position = new Vector3(x, y, 0);
    //     gamePiece.transform.rotation = Quaternion.identity;

    //     if (IsWithinBounds(x, y))
    //     {
    //         m_allGamePieces[x, y] = gamePiece;
    //     }

    //     gamePiece.SetCoord(x, y);
    // }

    private void RefillColumn(int column, float refillTime = 0.1f)
    {
        int falseYOffset = 0;
        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                RefillTile(column, i, falseYOffset, refillTime);
                falseYOffset++;
            }
        }
    }
    
    private void RefillTile(int x, int y, int falseYOffset, float refillTime = 0.1f)
    {
        int randomPiece = Random.Range(0,4);
        GameObject prefab = Instantiate(gamePiecePrefabs[randomPiece], new Vector3(x, height + falseYOffset, 0), Quaternion.identity);
        prefab.transform.parent = transform;
        GamePiece  piece = prefab.GetComponent<GamePiece>();
        piece.Init(this);
        piece.Move(x, y, refillTime*(height+falseYOffset-y));
        piece.SetCoord(x,y);
        m_allGamePieces[x,y] = piece;
    }

    private void CheckTntState()
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for(int i=0; i < width; i++){
            for(int j=0; j < height; j++){
                if(m_allGamePieces[i,j]!=null){
                    
                    if(!gamePieces.Contains(m_allGamePieces[i,j])){
                        List<GamePiece> matchingPieces = new List<GamePiece>();
                        List<GamePiece> breakablePieces = new List<GamePiece>();
                        (matchingPieces, breakablePieces) = FindAdjacentMatches(m_allGamePieces[i,j]);
                        if(matchingPieces.Count>=5){
                            foreach(var piece in matchingPieces){
                                if(piece.isMatchingPiece){
                                    piece.GetComponent<SpriteRenderer>().sprite = piece.tntStateSprite;
                                }
                            }
                        }
                        else{   // After a move, if the current tntstate of a piece is broken it should return its normal state
                            foreach(var piece in matchingPieces){
                                if(piece.isMatchingPiece){
                                    piece.GetComponent<SpriteRenderer>().sprite = piece.normalStateSprite;
                                }
                                
                            }
                        }
                        gamePieces = gamePieces.Union(matchingPieces).Union(breakablePieces).ToList();
                    }
                }
            }
        }
    }

    private void MakeTnt(int x, int y)
    {
        GameObject tntObject = Instantiate(gamePiecePrefabs[4], new Vector3(x,y,0), Quaternion.identity);
        tntObject.transform.parent = transform;
        GamePiece tnt = tntObject.GetComponent<GamePiece>();
        tnt.Init(this);
        tnt.SetCoord(x,y);
        m_allGamePieces[x,y] = tnt;
    }

    private (List<GamePiece>, List<GamePiece>) TriggerBomb(GamePiece clickedPiece)
    {
        int x = clickedPiece.xIndex;
        int y = clickedPiece.yIndex;
        bool tntCombo = false;
        if(IsWithinBounds(x+1,y)){
            if(m_allGamePieces[x+1,y].pieceType == PieceType.TNT) tntCombo=true;
        }
        if(IsWithinBounds(x-1,y)){
            if(m_allGamePieces[x-1,y].pieceType == PieceType.TNT) tntCombo=true;
        }
        if(IsWithinBounds(x,y+1)){
            if(m_allGamePieces[x,y+1].pieceType == PieceType.TNT) tntCombo=true;
        }
        if(IsWithinBounds(x,y-1)){
            if(m_allGamePieces[x,y-1].pieceType == PieceType.TNT) tntCombo=true;
        }

        // if there is a tnt adjacent to clicked tnt, tnt combo occurs whit size 7x7 
        int offset = tntCombo ? 3 : 2;

        List<GamePiece> matchingPieces = new List<GamePiece>();
        List<GamePiece> breakablePieces = new List<GamePiece>();
        // recursive function for searching all exploding game pieces
        void ChainTntReaction(int xIndex, int yIndex, int tntOffset=2){
            for(int i = xIndex-tntOffset; i<=xIndex+tntOffset;i++){
                for(int j = yIndex-tntOffset; j<=yIndex+tntOffset; j++){
                    if(IsWithinBounds(i,j)){
                        if(m_allGamePieces[i,j].isMatchingPiece){
                            if(!matchingPieces.Contains(m_allGamePieces[i,j])){
                                matchingPieces.Add(m_allGamePieces[i,j]);
                            }
                        }
                        else if(m_allGamePieces[i,j].isBreakable){
                            if(!breakablePieces.Contains(m_allGamePieces[i,j])){
                                breakablePieces.Add(m_allGamePieces[i,j]);
                            }
                        }
                        else if(m_allGamePieces[i,j].pieceType == PieceType.TNT){
                            if(!matchingPieces.Contains(m_allGamePieces[i,j])){
                                // Chain tnt reactions can only explode in 5x5 area so the tntoffset will remain 2
                                matchingPieces.Add(m_allGamePieces[i,j]);
                                ChainTntReaction(i,j);
                            }
                        }
                    }
                }
            }
        }
        ChainTntReaction(x, y, offset);
        matchingPieces.Add(clickedPiece);

        return (matchingPieces, breakablePieces);
    }
}
