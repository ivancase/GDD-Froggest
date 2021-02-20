using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockManager : MonoBehaviour
{

    #region Editor Variables
    [SerializeField]
    [Tooltip("The UI manager.")]
    private UIManager uiManager;

    [SerializeField]
    [Tooltip("Tadpole prefab.")]
    private Tadpole tadpolePrefab;

    [SerializeField]
    [Tooltip("Amount of tadpoles to create.")]
    private int tadpoleAmount = 10;

    [SerializeField]
    [Tooltip("Tadpole spawn radius.")]
    private float spawnRadius = 5f;

    [SerializeField]
    [Tooltip("Amount to separate flocks.")]
    private float separationAmount = 0.5f;

    [SerializeField]
    [Tooltip("Range within a tadpole considers another its neighbor.")]
    private float neighborRange = 0.5f;

    [SerializeField]
    [Tooltip("Amount to prioritize separation.")]
    private float separationWeight = 1f;

    [SerializeField]
    [Tooltip("Amount to prioritize alignment.")]
    private float alignmentWeight = 1f;

    [SerializeField]
    [Tooltip("Amount to prioritize cohesion.")]
    private float cohesionWeight = 1f;

    [SerializeField]
    [Tooltip("The sound a tadpole makes when it dies.")]
    private AudioClip dieSound;
    #endregion

    #region Cached Components
    private AudioSource cc_AudioSource;
    #endregion


    #region Private Variables
    private List<Tadpole> tadpoles;
    #endregion

    #region Public Functions
    public void Kill(Tadpole tadpole) {
        cc_AudioSource.clip = dieSound;
        cc_AudioSource.Play();


        tadpoles.Remove(tadpole);
        tadpoleAmount -= 1;
        uiManager.UpdateProgress(tadpoleAmount);
        Destroy(tadpole.gameObject);
    }
    #endregion

    private void Awake() {
        cc_AudioSource = GetComponent<AudioSource>();
    }

	private void Start () {
        tadpoles = new List<Tadpole>();

        for (int i = 0; i < tadpoleAmount; i++) {
            Vector2 pos = new Vector2(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius));
            Tadpole tadpole = Instantiate(tadpolePrefab, pos, Quaternion.identity, transform);
            tadpoles.Add(tadpole);
        }
	}

    private void FixedUpdate() {
        foreach (Tadpole tadpole in tadpoles) {
            Vector2 separation = Separate(tadpole) * separationWeight;
            Vector2 alignment = Align(tadpole) * alignmentWeight;
            Vector2 cohesion = Cohere(tadpole) * cohesionWeight;

            tadpole.AddVelocity(separation + alignment + cohesion);
        }
    }

    private Vector2 Separate(Tadpole tadpole) {
        Vector2 velocity = Vector2.zero;
        foreach (Tadpole neighbor in tadpoles) {
            if (neighbor == tadpole) {
                continue;
            }
            float distance = Vector2.Distance(neighbor.transform.localPosition, tadpole.transform.localPosition);
            if (distance < separationAmount) {
                Vector3 vec = (neighbor.transform.localPosition - tadpole.transform.localPosition).normalized / distance;
                velocity -= new Vector2(vec.x, vec.y);
            } 
        }
        return (velocity / (tadpoleAmount - 1)).normalized;
    }
    private Vector2 Align(Tadpole tadpole) {
        Vector2 velocity = Vector2.zero;
        foreach (Tadpole neighbor in tadpoles) {
            if (neighbor == tadpole) {
                continue;
            }
            float distance = Vector2.Distance(neighbor.transform.localPosition, tadpole.transform.localPosition);
            if (distance < neighborRange) {
                velocity += neighbor.GetComponent<Rigidbody2D>().velocity;
            } 
        }

        return (velocity / (tadpoleAmount - 1)).normalized;
    }

    private Vector2 Cohere(Tadpole tadpole) {
        Vector2 centerOfMass = Vector2.zero;
        foreach (Tadpole neighbor in tadpoles) {
            if (neighbor == tadpole) {
                continue;
            }
            float distance = Vector2.Distance(neighbor.transform.localPosition, tadpole.transform.localPosition);
            if (distance < neighborRange) {
                Vector3 pos = neighbor.transform.localPosition;
                centerOfMass += new Vector2(pos.x, pos.y);
            } 
        }
        Vector3 vec = tadpole.transform.localPosition;
        return ((centerOfMass / (tadpoleAmount - 1)) - new Vector2(vec.x, vec.y)).normalized;
    }
}