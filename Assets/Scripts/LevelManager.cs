using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;

public class LevelInfo
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
        
    }

public class LevelManager : MonoBehaviour
{
    [TextArea(3,5)]
    [SerializeField] string[] levelJsonPath;
    [SerializeField] Board board;
    [SerializeField] int level=1;
    public LevelInfo currentLevel;

    public int boxCount=0;
    public int stoneCount=0;
    public int vaseCount=0;
    public int moveCount=0;

    public event Action onBlastOccurs;
    public event Action onClicked;

    GameManager gameManager;
    private void Awake() {
        gameManager = FindObjectOfType<GameManager>();
    }


    private void Start() {
        level = gameManager.level;
        currentLevel = LoadLevel(level);
        GetGoals();
        moveCount = currentLevel.move_count;
        board.onValidClick += ValidClick; 
    }

    public LevelInfo LoadLevel(int level)
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

    public void GetGoals(){
        foreach(var item in currentLevel.grid){
            switch(item){
                case "bo":
                    boxCount++;
                    break;
                case "s":
                    stoneCount++;
                    break;
                case "v":
                    vaseCount++;
                    break;
                default:
                    break;
            }
        }
    }

    public void CheckGoals()
    {
        onBlastOccurs();
        // if(boxCount==0 && stoneCount==0 && vaseCount==0){
        //     LevelComplete();
        // }
    }

    private void LevelComplete()
    {
        throw new NotImplementedException();
    }

    private void ValidClick()
    {
        moveCount--;
        if(onClicked!=null){
            onClicked();
        }
        if(moveCount<=0){
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");
    }
}
