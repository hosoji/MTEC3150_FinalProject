//using System.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    //private CharacterController controller;
    public Transform player;
    //private Vector3 targetPoint;
    private Vector3 directionToPlayer;

    private Vector3 lastKnownPosition;

    private bool patrolling = true;
    private bool playerFound = false;

    public float alertDuration = 5;
    private float timeSinceAlerted = 0;
    private float timeWaited = 0;
    public float waitDuration = 2;
    private bool isWaiting = false;
    public Transform [] waypoints;
    private Transform targetWaypoint;
    //private Vector3 targetPosition;
    private int waypointIndex = 0;

    private bool hasJumped = false;

    public float walkSpeed =1.5f;
    public float runSpeed = 3;

    public float viewAngle = 120;
    public float viewRange = 5;
    public float detectionRadius = 0.5f;

    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    //private Quaternion defaultRotation;

    //public float rotationSpeed = 5;
    //public float walkSpeed = 5;


    



    void Start()
    {
        //controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        SetNextTargetWaypoint(true);

        
    }

    private void SetNextTargetWaypoint(bool firstTime = false)
    {
        if (!firstTime)
        {
            waypointIndex++;   
        }

        if (waypointIndex >= waypoints.Length)
        {
            waypointIndex = 0;
        }

        targetWaypoint = waypoints[waypointIndex];
        agent.SetDestination(targetWaypoint.position);
        
    }


    void Update()
    {
        anim.SetFloat("Velocity", agent.desiredVelocity.magnitude);
        
        //targetPoint = new Vector3(player.position.x, transform.position.y, player.position.z);
        directionToPlayer = (player.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(directionToPlayer);

        if (patrolling)
        {
            Patrol();
        }

        if (PlayerDetected())
        {
            playerFound = true;
            patrolling = false;

            timeSinceAlerted = 0;
            timeWaited = 0;
            isWaiting = false;
            lastKnownPosition = player.position;
            //Debug.Log("Player found!");
            agent.SetDestination(lastKnownPosition);

              //transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rot, agent.angularSpeed * Time.deltaTime );
    
        } 

        if (playerFound)
        {
            if (timeSinceAlerted < alertDuration)
            {
                //Debug.Log("Looking for player");
                timeSinceAlerted += Time.deltaTime;
            }
            else
            {    
                //Debug.Log("Returning to patrol");
                playerFound = false;
                timeSinceAlerted = 0;
                patrolling = true;
                SetNextTargetWaypoint(true);
            }
            
        }

        if (agent.isOnOffMeshLink && !hasJumped)
        {
            anim.SetTrigger("Jump");
            hasJumped = true;
        }

        else if ( !agent.isOnOffMeshLink)
        {
            hasJumped = false;
            
        }




        agent.speed = playerFound? runSpeed : walkSpeed;

        // if (playerFound)
        // {
        //     agent.speed = walkSpeed;
        // }
        // else
        // {
        //     agent.speed = runSpeed;
        // }

        //controller.Move(transform.forward * walkSpeed * Time.deltaTime);

        
    }

    private void Patrol()
    {
        //Debug.Log("Patrolling");


        float dist = Vector3.Distance(transform.position, targetWaypoint.position);
        float buffer = 0.25f;

        if (dist <= buffer && !isWaiting)
        {
            //SetNextTargetWaypoint(true);
            isWaiting = true;

        }

        if (isWaiting)
        {
            if (timeWaited < waitDuration)
            {
                timeWaited += Time.deltaTime;
            }
            else
            {
                SetNextTargetWaypoint();
                timeWaited = 0;
                isWaiting = false;
   
            }
            
        }


        
    }

    private bool PlayerDetected()
    {
        bool result = false;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < viewAngle / 2)
        {
            if (Physics.Raycast(transform.position, directionToPlayer, viewRange, playerLayer))
            {
                result = true;

            }
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRadius)
        {
            result = true;
        }

        return result;   
    }

    public void PlayFootstep()
    {
        AudioManager.inst.PlayFootstep(audioSource);
    
        
    }


}
