using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreenUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelTMP;
    [SerializeField] Button levelButton;

    private void Start() {
        if(LevelContainer.level <= LevelContainer.totalLevel){
            levelTMP.text = "Level " + LevelContainer.level;
        }
        else{
            levelTMP.text = "Finished";
            levelButton.interactable=false;
        }
        
    }
}
