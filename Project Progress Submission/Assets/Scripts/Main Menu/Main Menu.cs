using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void play()
    {
        SceneManager.LoadScene("Main Game");
    }

    public void NewGame_()
    {
        //New Game+ Options

        //Save Screen Interface

        //Choose save slot

        //When finished, load new game interface here and preload squadlist
    }

    public void NewGame()
    {
        SceneManager.LoadScene("Transition Phase");

        UnitResourceManager.Gold = 2500;
        UnitResourceManager.Iron = 10;
        UnitResourceManager.MagicGems = 10;
        UnitResourceManager.Horses = 10;
        
        //Save screen interface

        //Choose save slot

        //When finished, load new game interface here
    }

    public void LoadGame()
    {
        //Load screen interface

        //When finished, load transition interface here
    }

    public void settings()
    {
        GlobalSettings reference = new();
        reference.setup();
    }

    public void quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void Update()
    {
        if(Input.GetKeyUp(GlobalSettings.ControlMap[SettingKey.QuickLoad])) Debug.Log("QuickLoad the game");
    }
}
