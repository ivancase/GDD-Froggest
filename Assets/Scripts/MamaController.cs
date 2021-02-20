using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MamaController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("Mama's tongue attack speed.")]
    private float attackSpeed = 1f;

    [SerializeField]
    [Tooltip("Mama's tongue cooldown time.")]
    private float coolDownTime = 1f;

    [SerializeField]
    [Tooltip("The player.")]
    private GameObject player;

    [SerializeField]
    [Tooltip("The sound Mama makes when she puts her tongue out.")]
    private AudioClip tongueSound;
    #endregion


    #region Cached Components
    private Rigidbody2D cc_Rigidbody;
    private LineRenderer cc_LineRenderer;
    private EdgeCollider2D cc_EdgeCollider;
    private AudioSource cc_AudioSource;
    #endregion

    #region Private Variables
    private Vector3 target;
    private Vector3 targetRotation;
    private Vector3 origin;
    private Vector3 originalRotation;
    private float lineInterpolation;
    private float rotateInterpolation;
    private bool isFiring;
    private bool isQueuing;
    private bool isRotating;
    private int attackCount;
    #endregion

    #region Helper Functions
    private Vector3 GetTarget() {
        if (player == null) {
            return origin;
        }

        float timeToHit = (player.transform.position - transform.position).magnitude / (attackSpeed / 2);
        Vector3 vel = player.GetComponent<Rigidbody2D>().velocity;
        Vector3 futureTarget = player.transform.position + vel * timeToHit;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (futureTarget - transform.position),
                                             futureTarget.magnitude, ~LayerMask.GetMask("Player", "Mama", "Tadpole"));
        
        if (hit) {
            return hit.point;
        }
        else {
            return futureTarget;
        }
    } 

    private IEnumerator QueueForAttack() {
        isQueuing = true;
        // Very simple attack pattern. Slow-slow-fast!
        attackCount++;
        if (attackCount % 3 == 0) {
            yield return new WaitForSeconds(coolDownTime / 2);
        }
        else {
            yield return new WaitForSeconds(coolDownTime);
        }
        isQueuing = false;

        isRotating = true;
        target = GetTarget();
        originalRotation = transform.up;
        targetRotation = target - transform.position;
        //transform.up = target - transform.position;
    }
    private IEnumerator QueueForRetract() {
        isQueuing = true;
        yield return new WaitForSeconds(coolDownTime / 2);
        isQueuing = false;

        isFiring = true;

        Vector2 temp = origin;
        origin = target;
        target = temp;
    }
    #endregion
    private void Awake() {
        cc_Rigidbody = GetComponent<Rigidbody2D>();
        cc_LineRenderer = GetComponent<LineRenderer>();
        cc_EdgeCollider = GetComponent<EdgeCollider2D>();
        cc_AudioSource = GetComponent<AudioSource>();

        origin = cc_LineRenderer.GetPosition(0);
    }

    private void Update() {
        if (isRotating) {
            rotateInterpolation += Time.deltaTime * attackSpeed;
            rotateInterpolation = Mathf.Clamp01(rotateInterpolation);

            transform.up = Vector3.Lerp(originalRotation, targetRotation, rotateInterpolation);

            if (rotateInterpolation == 1) {
                rotateInterpolation = 0;
                isRotating = false;
                isFiring = true;

                cc_AudioSource.clip = tongueSound;
                cc_AudioSource.Play();
            }
        }
        else if (isFiring) {
            lineInterpolation += Time.deltaTime * attackSpeed;
            lineInterpolation = Mathf.Clamp01(lineInterpolation);

            Vector3 lerpedTarget = Vector3.Lerp(origin, target, lineInterpolation);
            cc_LineRenderer.SetPosition(1, lerpedTarget);

            Vector2[] points = cc_EdgeCollider.points;
            points[1] = transform.InverseTransformPoint(lerpedTarget);
            cc_EdgeCollider.points = points;

            if (lineInterpolation == 1) {
                lineInterpolation = 0;
                isFiring = false;
                if (target == cc_LineRenderer.GetPosition(0)) {
                    origin = target;
                }
                else {
                    StartCoroutine("QueueForRetract");
                }
            }
        }
        else if (!isQueuing) {
            StartCoroutine("QueueForAttack");
        }  
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player") {
            Destroy(other.gameObject);
        }
    }
}
