using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void StartLevel(){
        // loads the game scene
        SceneManager.LoadScene(1);
    }

    public void TryAgain(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnMainScreen(){
        SceneManager.LoadScene(0);
    }

    
}
