using NUnit.Framework;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

public class FPC : MonoBehaviour
{
    public float walkSpeed = 5;
    public float sprintSpeed = 10;
    private float currentSpeed;

    public float jumpForce = 5;

    public float throwForce = 5;

    private float gravity = 9.81f; 
    public float mouseSensitivity = 2;
    public float upDownRange = 80;

    public float pickUpRange = 2;
    public Transform holdPoint;
    private Item heldItem;

    private Vector3 currentMovement;

    private Vector3 hitPoint;
    public ParticleSystem impactPS;
    [UnityEngine.Range(10,30)] public int particleCount = 20;

    private float verticalRotation;
    private CharacterController characterController;
    private Camera mainCam;

    public AudioClip shootingSound;
    public AudioClip impactSound;

    private AudioSource audioSource;
    float walkStepInterval = 0.5f;
    float runStepInterval = 0.3f;
    float currentStepInterval;

    bool isMoving;
    bool isSprinting;
    float nextTimeStep;
    float velocityThreshold = 2;

    //public Vector3 ObjectinFocus {get{return GetObjectInFocus();}}


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        mainCam = Camera.main;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    void Update()
    {
        Movement();
        MouseLook();
        Sprinting();
        Jumping();


        if (heldItem != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                heldItem.Throw(throwForce, mainCam.transform.forward);
                heldItem = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //AudioManager.inst.PlaySoundAtPosition(shootingSound,transform.position);
        }



        if (ObjectInFocus() != null)
        {
            float distanceToObject = Vector3.Distance(mainCam.transform.position,ObjectInFocus().transform.position);

            //print(ObjectInFocus().name);
            if (Input.GetMouseButtonDown(0))
            {
                impactPS.transform.position = hitPoint;
                impactPS.Emit(particleCount);
                AudioManager.inst.PlaySoundAtPosition(impactSound,hitPoint);
            }

            if (distanceToObject <= pickUpRange && ObjectInFocus().GetComponent<Item>() != null)
            {
                if(Input.GetMouseButtonDown(1))
                {
                    heldItem = ObjectInFocus().GetComponent<Item>();
                    heldItem.PickUp(mainCam.transform, holdPoint.position);          
                }
            }

        }


    }


    void Movement()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalSpeed = verticalInput * currentSpeed;
        float horizontalSpeed = horizontalInput * currentSpeed;

        Vector3 horizontalMovement = new Vector3(horizontalSpeed,0,verticalSpeed);
        horizontalMovement = transform.rotation * horizontalMovement;
        currentMovement.x = horizontalMovement.x;
        currentMovement.z = horizontalMovement.z;

        characterController.Move(currentMovement * Time.deltaTime);

   
        isMoving = verticalInput != 0 || horizontalInput != 0;
        HandleFootsteps();

        
    }

    void Sprinting()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;

        }else
        {
            currentSpeed = walkSpeed;
            isSprinting = false;
        }
    }

    void Jumping()
    {
        if (characterController.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
        
    }

    void MouseLook()
    {
        float mouseXrotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0,mouseXrotation,0);
 
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation,-upDownRange,upDownRange);
        mainCam.transform.localRotation = Quaternion.Euler(verticalRotation,0,0);

        
        
    }

    public GameObject ObjectInFocus()
    {
        GameObject result = null;
        RaycastHit hit;

        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit))
        {
            result = hit.transform.gameObject;
            hitPoint = hit.point;
        }

        
        return result;
        
    }

    private void HandleFootsteps()
    {
        currentStepInterval = isSprinting? runStepInterval: walkStepInterval;

        if (characterController.isGrounded && isMoving && 
            Time.time > nextTimeStep && characterController.velocity.magnitude > velocityThreshold)
        {
            AudioManager.inst.PlayFootstep(audioSource);
            nextTimeStep = Time.time + currentStepInterval;     
            
        }

    }



}
