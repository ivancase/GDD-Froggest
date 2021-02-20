using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("The rate of change of force applied of the player.")]
    private float appliedForce = 0.25f;

    [SerializeField]
    [Tooltip("The maximum total force applied to the player when swimming.")]
    private float maxForce = 1f;

    [SerializeField]
    [Tooltip("The minimum value of Linear Drag.")]
    private float minDrag = 1f;

    [SerializeField]
    [Tooltip("The maximum value of Linear Drag.")]
    private float maxDrag = 2f;

    [SerializeField]
    [Tooltip("The button the player must hold to move forwards.")]
    private string moveButton = "Fire1";

    [SerializeField]
    [Tooltip("The button the player must hold to charge up their attack.")]
    private string chargeButton = "Fire2";

    [SerializeField]
    [Tooltip("The force of a fully charged attack.")]
    private float chargeMaxForce = 10f;

    [SerializeField]
    [Tooltip("Time the charge button must be held to do a fully charged attack.")]
    private float chargeTime = 2f;

    [SerializeField]
    [Tooltip("Animation speed multiplayer.")]
    private float animationSpeed = 1f;

    [SerializeField]
    [Tooltip("The names of the triggers for each phase, in order.")]
    private string[] phaseTriggers;

    [SerializeField]
    [Tooltip("The sound the player makes when dashing.")]
    private AudioClip dashSound;

    [SerializeField]
    [Tooltip("The sound the player makes when evolving.")]
    private AudioClip evolveSound;
    #endregion
    
    #region Cached Components
    private Rigidbody2D cc_Rigidbody;
    private Animator cc_Animator;
    private AudioSource cc_AudioSource;
    #endregion

    #region Private Variables
    private float force;
    private Vector3 direction;

    private float chargeForce;
    private float chargeInterpolation;
    private bool isCharged;

    private int currentPhase;
    #endregion

    #region Public Functions
    public void NextPhase() {
        if (currentPhase == phaseTriggers.Length) {
            Debug.LogError("Trying to change phase when on final phase.");
            return;
        }
        cc_AudioSource.clip = evolveSound;
        cc_AudioSource.Play();

        cc_Animator.SetTrigger(phaseTriggers[currentPhase]);
        currentPhase++;
        transform.localScale *= 1.25f;
    }
    #endregion

    private void Awake() {
        cc_Rigidbody = GetComponent<Rigidbody2D>();
        cc_Animator = GetComponent<Animator>();
        cc_AudioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.right = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;


        if (Input.GetButton(chargeButton)) {
            chargeInterpolation += Time.deltaTime / chargeTime;
            chargeInterpolation = Mathf.Clamp01(chargeInterpolation);
            chargeForce = Mathf.Lerp(0f, chargeMaxForce, chargeInterpolation);

            cc_Animator.speed = chargeForce * animationSpeed / 100;

            force = 0;
        }
        else if (Input.GetButtonUp(chargeButton)) {
            isCharged = true;
        }
        else if (Input.GetButton(moveButton)) {
            force = Mathf.Clamp(force + appliedForce, 0f, maxForce);
            direction = transform.right;
        } 
        else {
            force = 0;
        }
    }

    private void FixedUpdate() {
        if (isCharged) {
            cc_AudioSource.clip = dashSound;
            cc_AudioSource.Play();

            cc_Rigidbody.AddForce(transform.right * chargeForce);
            direction = transform.right;

            isCharged = false;
            chargeForce = 0;
            chargeInterpolation = 0;
        }

        cc_Rigidbody.AddForce(transform.right * force);
        cc_Animator.speed = cc_Rigidbody.velocity.magnitude * animationSpeed;

        // If you stop acceleration and do a sharp turn, you get increased drag to slow down faster.
        Vector2 diff = direction - transform.right;
        if (Mathf.Abs(diff.x) > 0.75f || Mathf.Abs(diff.y) > 0.75f) {
            cc_Rigidbody.drag = maxDrag;
        } 
        else {
            cc_Rigidbody.drag = minDrag;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Untagged") {
            direction = -direction;
        }
    }


}
