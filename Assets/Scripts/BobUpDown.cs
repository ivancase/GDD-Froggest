using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobUpDown : MonoBehaviour {
    #region Editor Variables
    [SerializeField]
    [Tooltip("Maximum height.")]
    private float maxHeight = 10f;

    [SerializeField]
    [Tooltip("Minimum height.")]
    private float minHeight = 10f;

    [SerializeField]
    [Tooltip("The bobbing speed.")]
    private float speed = 1f;
    #endregion

    #region Private Variables
    private Vector2 upGoal;
    private Vector2 downGoal;
    private float interpolation;
    private bool goingUp;
    #endregion

    private void Awake() {
        upGoal = new Vector2(transform.position.x, transform.position.y + maxHeight);
        downGoal = new Vector2(transform.position.x, transform.position.y - minHeight);
    }

    private void Update() {
        interpolation += Time.deltaTime * speed;
        if (interpolation >= 1) {
            interpolation = 0;
            goingUp = !goingUp;
        }

        if (goingUp) {
            transform.position = Vector2.Lerp(upGoal, downGoal, interpolation);
        }
        else {
            transform.position = Vector2.Lerp(downGoal, upGoal, interpolation);
        }
    }
}
