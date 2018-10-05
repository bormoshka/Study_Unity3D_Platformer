using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : UIController {

	public Text HighscoreValueText;
	public Text scoreValueText;
	public Text title;

	void Start() {
		HighscoreValueText.text = GameManager.instance.highscore + "";
		scoreValueText.text = GameManager.instance.score + "";
		title.text = GameManager.instance.isComplete ? "You've made it!" : "GAME OVER";
	}
	
	public void StartGame() {
		base.StartGame();
		GameManager.instance.ResetGame();
	}
	
	
}
