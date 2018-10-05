using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public int score = 0;
    public int highscore = 0;
    public int currentLevel = 1;
    public int totalLevels = 2;
    public HudManager HudManager;
    public bool isComplete = false;

    public static GameManager instance;

    public GameManager getInstance() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        return instance;
    }

    void Awake() {
        getInstance();
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount) {
        score += amount;
        if (highscore < score) {
            highscore = score;
            HudManager.updateHighScore(highscore);
        }

        HudManager.updateScore(score);
    }

    public void GameOver() {
        SceneManager.LoadScene("GameOver");
    }

    public void ResetGame() {
        currentLevel = 1;
        HudManager.clear();
        score = 0;
        isComplete = false;
    }

    public void NextLevel() {
        if (currentLevel == totalLevels) {
            isComplete = true;
            GameOver();
            return;
        }

        goToLevel(++currentLevel);
    }

    private void goToLevel(int level) {
        SceneManager.LoadScene("Level-" + (level < 10 ? "0" : "") + +level);
    }
}