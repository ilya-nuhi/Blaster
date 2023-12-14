using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] GameObject[] goalVarietyObjects;

    [SerializeField] Sprite[] breakableSprites;
    [SerializeField] LevelManager levelManager;
    [SerializeField] TextMeshProUGUI moveCountTMP;
    [SerializeField] GameObject failPopup;
    [SerializeField] GameObject celebration;
    List<TextMeshProUGUI> goalCountTexts = new List<TextMeshProUGUI>();
    List<Image> goalCheckImages = new List<Image>();

    public bool isBoxGoal = false;
    public bool isStoneGoal = false;
    public bool isVaseGoal = false;
    private void Start() {
        SetGoalSprites();
        levelManager.onBlastOccurs+=UpdateUI;
        levelManager.onClicked+=UpdateMoveCount;
        levelManager.onFail+=ShowFailPopup;
        levelManager.onLevelSuccess+=NextLevelAnim;
        UpdateUI();
        UpdateMoveCount();

    }

    void SetGoalSprites(){      // setting goal images and texts according to how many different breakables in the current level.
        List<Sprite> goalSprites = new List<Sprite>();
        if(levelManager.boxCount>0){
            goalSprites.Add(breakableSprites[0]);
            isBoxGoal=true;
        }
        if(levelManager.stoneCount>0){
            goalSprites.Add(breakableSprites[1]);
            isStoneGoal=true;
        }
        if(levelManager.vaseCount>0){
            goalSprites.Add(breakableSprites[2]);
            isVaseGoal=true;
        }

        if(goalSprites.Count==0){
            Debug.LogWarning("WARNING! No Goal Pieces Found.");
        }else{
            goalVarietyObjects[goalSprites.Count-1].SetActive(true);    // activating gameobject for placing goal sprites by variety of breakables.
            for(int i = 0; i< goalSprites.Count; i++){
                if(goalVarietyObjects[goalSprites.Count-1].transform.GetChild(i)!=null){
                    Transform childObject = goalVarietyObjects[goalSprites.Count-1].transform.GetChild(i);
                    childObject.GetComponent<Image>().sprite = goalSprites[i];

                    // finding corresponding TMP and image components in grandchildren
                    goalCountTexts.Add(childObject.GetComponentInChildren<TextMeshProUGUI>());
                    for(int j = 0; j < childObject.transform.childCount; j++){
                        if(childObject.transform.GetChild(j).GetComponent<Image>()!=null){
                            goalCheckImages.Add(childObject.transform.GetChild(j).GetComponent<Image>());
                        }
                    }
                }
            }
        }
    }

    void UpdateUI(){   
        int index = 0;
        if(isBoxGoal){
            goalCountTexts[index].text = levelManager.boxCount.ToString();
            if(levelManager.boxCount==0){
                goalCountTexts[index].gameObject.SetActive(false);

                goalCheckImages[index].gameObject.SetActive(true);
            }
            index++;
        }
        if(isStoneGoal){
            goalCountTexts[index].text = levelManager.stoneCount.ToString();
            if(levelManager.stoneCount==0){
                goalCountTexts[index].gameObject.SetActive(false);
                goalCheckImages[index].gameObject.SetActive(true);
            }
            index++;
        }
        if(isVaseGoal){
            goalCountTexts[index].text = levelManager.vaseCount.ToString();
            if(levelManager.vaseCount==0){
                goalCountTexts[index].gameObject.SetActive(false);
                goalCheckImages[index].gameObject.SetActive(true);
            }
        }
    }

    private void UpdateMoveCount()
    {
        moveCountTMP.text = levelManager.moveCount.ToString();
    }
    void ShowFailPopup(){
        failPopup.SetActive(true);
    }

    private void NextLevelAnim()
    {
        celebration.SetActive(true);
    }

}
