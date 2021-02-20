using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tadpole : MonoBehaviour
{

    #region Editor Variables
    [SerializeField]
    [Tooltip("Maximum speed.")]
    private float maxSpeed = 5f;

    [SerializeField]
    [Tooltip("The range to avoid the player.")]
    private float avoidRange = 1f;

    [SerializeField]
    [Tooltip("The amount to prioritize player avoidance.")]
    private float avoidWeight = 1f;
    #endregion

    #region Cached Components
    private Rigidbody2D cc_Rigidbody;
    #endregion

    #region Private Variables
    private FlockManager flockManager;
    #endregion

    #region Helper Functions
    private Vector2 Avoid() {
        Collider2D collide = Physics2D.OverlapCircle(transform.position, avoidRange, LayerMask.GetMask("Player"));
        if (collide) {  
            Transform avoidee = collide.transform;
            float distance = Vector2.Distance(avoidee.position, transform.position);

            Vector3 vec = (avoidee.position - transform.position).normalized / distance;
            return -(new Vector2(vec.x, vec.y) * avoidWeight);
        }

        return Vector2.zero;

        /*RaycastHit2D hit = Physics2D.CircleCast(transform.position, avoidRange, Vector2.zero, ~LayerMask.GetMask("Frog"));
        if (hit) {
            float distance = Vector2.Distance(hit.point, transform.position);

            Vector2 pos = transform.position;
            cc_Rigidbody.velocity -= (hit.point - pos).normalized / distance;;
        }*/
    }
    #endregion

    #region Public Functions
    public void AddVelocity(Vector2 velocity) {
        cc_Rigidbody.velocity += velocity;
    }
    #endregion
    private void Awake() {
        cc_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        cc_Rigidbody.velocity = new Vector3(Random.value * 2 - 1, Random.value * 2 - 1);
        flockManager = transform.parent.gameObject.GetComponent<FlockManager>();
    }
    private void FixedUpdate()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 90) * cc_Rigidbody.velocity);

        AddVelocity(Avoid());

        if(cc_Rigidbody.velocity.magnitude > maxSpeed) {
            cc_Rigidbody.velocity = cc_Rigidbody.velocity.normalized * maxSpeed;
        }
    }
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player") {
            flockManager.Kill(this);
        }
    }

}
