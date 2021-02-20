using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("The player.")]
    private PlayerController player;

    [SerializeField]
    [Tooltip("The progress bar.")]
    private RectTransform progressBar;

    [SerializeField]
    [Tooltip("The restart tip.")]
    private Text restartText;

    [SerializeField]
    [Tooltip("Amount of checkpoints (including victory point).")]
    private int checkpointAmount = 4;

    [SerializeField]
    [Tooltip("Button to press to restart.")]
    private string restartButton = "Jump";
    #endregion

    #region Private Variables
    private float progressBarOrigWidth; 
    private float progress;
    private float goalProgress;
    private bool playerIsDead;
    #endregion

    #region Public Functions
    public void UpdateProgress(int total) {
        progress++;

        if (goalProgress == 0) {
            goalProgress = (total / checkpointAmount) + 1;
        }

        progressBar.sizeDelta += new Vector2(progressBarOrigWidth / goalProgress, 0);

        if (total == 0) {
            EndGame();
            return;
        }

        if (progress == goalProgress) {
            player.NextPhase();

            progress = 0;
            goalProgress = 0;
            checkpointAmount -= 1;
            progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);
        }
    }

    public void EndGame() {
        SceneManager.LoadScene("Victory");
    }

    #endregion

    private void Awake() {
        progressBarOrigWidth = progressBar.sizeDelta.x;
        progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);

        restartText.enabled = false;
    }

    private void Update() {
        if (playerIsDead) {
            if (Input.GetButtonDown(restartButton)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else {
            if (player == null) {
                playerIsDead = true;
                restartText.enabled = true;
            }
        }
    }

}
