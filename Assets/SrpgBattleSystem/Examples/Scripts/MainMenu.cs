﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnClickRectangle()
    {
        SceneManager.LoadScene("Rectangle");
    }
}