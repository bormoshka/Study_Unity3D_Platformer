using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour {
    public Text scoreText;
    public Text highScoreText;
    public Canvas gameCanvas;

    // Use this for initialization
    public void updateScore(int score) {
        scoreText.text = score + "";
    }

    public void updateHighScore(int score) {
        highScoreText.text = score + "";
    }

    public void clear() {
        scoreText.text = "0";
        //highScoreText.text = "0";
    }
}