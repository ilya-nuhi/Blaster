using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] SoundManager soundManager;
    [SerializeField] GameManager gameManager;
    public LevelInfo currentLevel;

    public int boxCount=0;
    public int stoneCount=0;
    public int vaseCount=0;
    public int moveCount=0;

    public event Action onBlastOccurs;
    public event Action onClicked;
    public event Action onFail;
    public event Action onLevelSuccess;
    public bool levelEnd = false;
    int level=1;
    bool levelSuccess = false; // this variable is for preventing conflict with last move win.
    


    private void Start() {
        level = LevelContainer.level;
        LevelContainer.totalLevel = levelJsonPath.Length;
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
        if(boxCount==0 && stoneCount==0 && vaseCount==0){   // level is finished
        soundManager.SuccessSound();
            levelEnd = true;
            levelSuccess = true;
            if(onLevelSuccess!=null){
                onLevelSuccess();
            }
            LevelContainer.level++;
        }
    }

    private void ValidClick()
    {
        soundManager.PopSound();
        moveCount--;
        if(onClicked!=null){
            onClicked();
        }
        StartCoroutine(CheckGameOver());
    }

    IEnumerator CheckGameOver()
    {
        yield return new WaitForEndOfFrame();
        if(moveCount<=0){
            levelEnd = true;
            if(!levelSuccess){
                if(onFail!=null){
                    onFail();
                }
            }
        }
    }

    void OnClick(){
        if(levelSuccess){
            // returns main menu on click when level is completed
            SceneManager.LoadScene(0);
        }
    }

}
