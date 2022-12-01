using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class Beholder : MonoBehaviour
{
    public RoomManager roomManager;
    public Rigidbody2D rb;

    [SerializeField]
    private Animator rotationAnimator;

    [SerializeField]
    private AIDestinationSetter ai;
    [SerializeField]
    private IgnoreParentPosition iPP;

    [SerializeField]
    private float
        movementSpeed,
        rotationSpeed,
        laserInterval;

    private int positionIndex;

    private bool isInPosition;

    public Transform pathfindingTarget;

    [SerializeField]
    private List<GameObject> lasers = new List<GameObject>();

    private List<Vector2> positions = new List<Vector2>();

    private void Awake()
    {
        isInPosition = false;
        findingPosition = false;
        attacking = false;
        positionIndex = 0;

        rb = this.GetComponent<Rigidbody2D>();
        roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        ai = this.GetComponent<AIDestinationSetter>();
    }

    private void OnEnable()
    {
        ai.enabled = false;
        FindPosition();
    }
    // Update is called once per frame
    void Update()
    {
        if (isInPosition)
        {
            AttackPattern();
        }
    }

    private void FindPosition()
    {
        if (positions.Count == 3)
        {
            iPP.position = positions[positionIndex];

            StartCoroutine(FindingPosition(positions[positionIndex]));

            if (positionIndex == 2)
            {
                positionIndex = 0;
            }
            else
            {
                positionIndex++;
            }
        }
        else
        {
            int index = Random.Range(0, roomManager.FloorTiles.Count);

            HashSet<Vector2Int> availablePositions = new HashSet<Vector2Int>(roomManager.FloorTiles);

            availablePositions.ExceptWith(roomManager.CounterPositions);

            //get a random avaliable floor tile in roomManager and move there
            Vector2 position = roomManager.TileFromWorldPosition(availablePositions.ElementAt(index));

            positions.Add(position);

            iPP.position = position;

            StartCoroutine(FindingPosition(position));
        }

    }

    bool findingPosition;
    IEnumerator FindingPosition(Vector2 position)
    {
        ai.enabled = true;

        findingPosition = true;

        while(findingPosition)
        {
            //transform.position = Vector2.Lerp(startPosition, position, movementSpeed);

            //set seeker target to position
            ai.target = pathfindingTarget;

            float distanceToTarget = Vector3.Distance(position, transform.position);

            if (distanceToTarget < 0.1f)
            {
                print("made it to position");

                findingPosition = false;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        ai.enabled = false;
        isInPosition = true;
    }

    private void AttackPattern()
    {
        if (!attacking)
        {
            print("attacking");

            StartCoroutine(Attacking());
        }
    }

    bool attacking;
    IEnumerator Attacking()
    {
        attacking = true;

        float timer = 0;

        rotationAnimator.Play("Rotate");

        while (attacking)
        {

            timer += Time.deltaTime;

            if (timer >= laserInterval)
            {
                attacking = false;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        isInPosition = false;
        rotationAnimator.Play("Default");
        FindPosition();
    }

}
