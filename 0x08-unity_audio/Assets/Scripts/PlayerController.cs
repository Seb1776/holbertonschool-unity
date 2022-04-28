using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header ("Movement")]
    public float moveSpeed;
    public float runSpeed;
    public float moveToRunDelta;
    public float moveSpeedMultiplier;
    public float jumpForce;
    public float airMultiplier;
    public bool canMove;
    public bool canRotate;
    public bool useController;
    public bool invertY;
    public float timeBtwFootSteps;
    public AudioClip footsStepClipRock, footsStepsClipGrass,
        landRock, landGrass;
    public float platformDetectLength;
    public LayerMask whatIsPlatform;
    [SerializeField]
    bool onGround;
    [Header ("Physics")]
    public Transform mainCam;
    public float rbDrag;
    public float airDrag;
    public float fixeScale;
    public float playerHeight;
    public LayerMask isGround;
    [Header ("Respawn Stuff")]
    public Transform spawnPos;
    public MusicSystem musicSystem;
    public float yOffset;
    [Header ("Other")]
    public AudioClip clearStageSFX;

    float currentSpeed;
    float horizontalMv;
    float verticalMv;
    float nextStep;
    float timeNotOnGround;
    bool fall;
    Rigidbody rb;
    Vector3 moveDirection;
    AudioClip currentSteps, currentLand;
    AudioSource sfxSource;
    [SerializeField] PauseMenu pause;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sfxSource = GetComponent<AudioSource>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LoadConfig();
    }

    void Update()
    {
        GetInputs();
        ControlDrag();
        GetPlayerDirection();
        DetectDifferentPlatform();
    }

    void LoadConfig()
    {   
        if (File.Exists(Application.persistentDataPath + "/config.run"))
        {
            ConfigData cd = SaveSystem.LoadConfig();
            invertY = cd.invertY;
        }
    }

    void FixedUpdate() 
    {
        GetMovement();
    }

    void DetectDifferentPlatform()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, platformDetectLength, whatIsPlatform))
            if (hit.collider != null)
                if (hit.collider.tag.Contains("Plat"))
                    CurrentSteps(hit.transform.tag);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * platformDetectLength);
    }

    void GetPlayerDirection()
    {
        moveDirection = transform.forward * verticalMv + transform.right * horizontalMv;

        if (canRotate)
            transform.Rotate(0f, Input.GetAxis("Mouse X") * mainCam.GetComponent<CameraController>().sensitivity, 0f);
        
        if (canMove)
        {
            if (onGround)
            {
                rb.AddForce(moveDirection.normalized * currentSpeed * moveSpeedMultiplier, ForceMode.Acceleration);

                if (moveDirection.magnitude > 0.1f && (Time.time > nextStep && timeNotOnGround == 0f))
                {
                    nextStep = timeBtwFootSteps + Time.time;
                    sfxSource.PlayOneShot(currentSteps);
                }

                else if (timeNotOnGround > 0f)
                {
                    sfxSource.PlayOneShot(currentLand);
                    timeNotOnGround = 0f;
                }
            }
                
            else
            {
                rb.AddForce(moveDirection.normalized * currentSpeed * moveSpeedMultiplier * airMultiplier, ForceMode.Acceleration);
                timeNotOnGround += Time.deltaTime;
            }
        }
    }

    void GetInputs()
    {
        horizontalMv = Input.GetAxisRaw("Horizontal");
        verticalMv = Input.GetAxisRaw("Vertical");
        rb.drag = rbDrag;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1))
            GetJump();

        onGround = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2f + .1f, isGround);
    }

    public void CurrentSteps(string step)
    {
        if (step == "GrassPlat")
        {
            currentSteps = footsStepsClipGrass;
            currentLand = landGrass;
        }
        
        else
        {
            currentSteps = footsStepClipRock;
            currentLand = landRock;
        }
    }

    void GetMovement()
    {     
        if (canMove)
        {
            if (onGround)
            {   
                if (Mathf.Abs(horizontalMv) > 0 || Mathf.Abs(verticalMv) > 0)
                {   
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Joystick1Button0))
                        currentSpeed = Mathf.Lerp(currentSpeed, runSpeed, moveToRunDelta);
                    else
                        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, moveToRunDelta);
                }

                else if (Mathf.Abs(horizontalMv) <= 0 && Mathf.Abs(verticalMv) <= 0)
                {
                    currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, moveToRunDelta);
                }
            }
        }
    }

    void ControlDrag()
    {
        if (onGround)
            rb.drag = rbDrag;

        else
            rb.drag = airDrag;
    }

    void GetJump()
    {
        if (onGround)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResetPos"))
            Respawn();
        
        if (other.CompareTag("WinFlag"))
        {
            sfxSource.PlayOneShot(clearStageSFX);
            musicSystem.StopMusic();
            pause.canPause = false;
            GetComponent<Timer>().TriggerWinGame();
        }
    }

    public void RetryGame()
    {
        canRotate = true;
        GetComponent<Timer>().winPanel.SetActive(false);
        GetComponent<Timer>().startPlaying.SetActive(true);
        GetComponent<Timer>().canCount = false;
        GetComponent<Timer>().winGame = false;
        GetComponent<Timer>().timerTime = 0f;
        GetComponent<Timer>().timer.text = "0:00:00";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        musicSystem.ReplayMusic();
        Respawn();
    }

    public void Respawn()
    {
        rb.velocity = rb.angularVelocity = Vector3.zero;
        StartCoroutine(DisableMovement(2f));
        transform.position = new Vector3(spawnPos.position.x, spawnPos.position.y + yOffset, spawnPos.position.z);
    }

    IEnumerator DisableMovement(float duration)
    {
       canMove = /*canRotate =*/ false;
       yield return new WaitForSeconds(duration);
       canMove = /*canRotate =*/ true;
    }
}
