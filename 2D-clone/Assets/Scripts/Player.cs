using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class Player : MonoBehaviour
{
    [Header("Player")]
    public float moveSpeed;
    public bool canMove = true;
    public Node.ValidDirections startingDirection;
    public Vector2 orientation;
    public Node startingNode;
    public int pacManLives;
    [Header("SFX")]
    public AudioClip[] munchSFX;
    public AudioClip superPelletEat;
    [Header ("PS4 LightBar Animation")]
    public bool overrideAnimSpeed;
    public bool useController;

    AudioSource source;
    bool _munch;
    bool controllerAvailable;
    Node currentNode, targetNode, previousNode;
    SpriteRenderer pacmanSprite;
    GameManager manager;
    InputMaster inputActions;
    Vector2 playerInputVector, nextDirection, gamepadAxis;
    Animator animator;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        pacmanSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();
        controllerAvailable = Gamepad.all.Count > 0;

        inputActions = new InputMaster();
        inputActions.Enable();
        inputActions.Player.Movement.performed += CheckJoystickInput;
    }

    void Start()
    {
        Node node = GetNodeAtPosition(transform.localPosition);

        if (node != null)
        {
            currentNode = startingNode = node;
        }
        
        playerInputVector = currentNode.ConvertDirectionFromEnum(startingDirection);
        orientation = currentNode.ConvertDirectionFromEnum(startingDirection);
        ChangePosition(playerInputVector);

        if (!controllerAvailable && useController)
            useController = false;
    }

    void Update()
    {   
        if (canMove)
        {
            HandlePS4LightBar();
            Move();
            UpdateRotation();
            CheckInput();
            HandleAnimations();
            EatPellet();

            if (manager.createdFruit != null)
                CheckCollisions();
        }
    }

    Node GetNodeAtPosition(Vector2 pos)
    {
        GameObject tile = manager.boardObjects[(int)pos.x, (int)pos.y];

        if (tile != null)
        {
            return tile.GetComponent<Node>();
        }

        return null;
    }

    void CheckCollisions()
    {
        Rect playerRect = new Rect(transform.position, transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.bounds.size / 16f);
        Rect fruitRect = new Rect(manager.createdFruit.transform.position, manager.createdFruit.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.bounds.size / 115f);

        if (playerRect.Overlaps(fruitRect))
        {
            source.PlayOneShot(manager.createdFruit.GetComponent<Fruit>().collectedFruit);
            manager.createdFruit.GetComponent<Fruit>().CollectedFruit();
        }
    }

    void HandleAnimations()
    {
        animator.SetBool("moving", (playerInputVector == Vector2.zero) ? false: true);
    }

    void Move ()
    {
        if (targetNode != currentNode && targetNode != null)
        {
            if (nextDirection == playerInputVector * -1f)
            {
                playerInputVector *= -1f;

                Node tempNode = targetNode;
                targetNode = previousNode;
                previousNode = tempNode;
            }

            if (OverShotTarget())
            {
                currentNode = targetNode;

                transform.localPosition = currentNode.transform.position;

                Transform _portalEnd = GetPortal(currentNode.transform.position);

                if (_portalEnd != null)
                {
                    transform.localPosition = _portalEnd.position;
                    currentNode = _portalEnd.GetComponent<Node>();
                }

                Node moveToNode = CanMove(nextDirection);

                if (moveToNode != null)
                    playerInputVector = nextDirection;
                
                if (moveToNode == null)
                    moveToNode = CanMove(playerInputVector);
                
                if (moveToNode != null)
                {
                    targetNode = moveToNode;
                    previousNode = currentNode;
                    currentNode = null;
                }

                else
                    playerInputVector = Vector2.zero;
            }

            else
                transform.localPosition += (Vector3)playerInputVector * moveSpeed * Time.deltaTime;
        }
    }

    void ChangePosition(Vector2 d)
    {
        if (d != playerInputVector)
            nextDirection = d;
        
        if (currentNode != null)
        {
            Node moveToNode = CanMove(d);
        
            if (moveToNode != null)
            {
                playerInputVector = d;
                targetNode = moveToNode;
                previousNode = currentNode;
                currentNode = null;
            }
        }
    }

    void MoveToNode(Vector2 d)
    {
        Node moveToNode = CanMove(d);

        if (moveToNode != null)
        {
            transform.localPosition = moveToNode.transform.localPosition;
            currentNode = moveToNode;
        }
    }

    Node CanMove(Vector2 d)
    {
        Node moveToNode = null;

        for (int i = 0; i < currentNode.neighbours.Count; i++)
        {   
            if (currentNode.ConvertDirectionFromEnum(currentNode.validDirections[i]) == d)
            {
                moveToNode = currentNode.neighbours[i];
                break;
            }
        }

        return moveToNode;
    }

    void UpdateRotation()
    {
        if (playerInputVector == Vector2.right)
        {
            pacmanSprite.flipX = false;
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        
        else if (playerInputVector == Vector2.left)
        {
            pacmanSprite.flipX = true;
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        
        else if (playerInputVector == Vector2.up)
        {
            pacmanSprite.flipX = false;
            transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        else if (playerInputVector == Vector2.down)
        {
            pacmanSprite.flipX = false;
            transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
        }

        orientation = playerInputVector;
    }

    void CheckInput()
    {   
        if (!useController)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                ChangePosition(Vector2.left);
            
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                ChangePosition(Vector2.right);
            
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                ChangePosition(Vector2.down);
            
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                ChangePosition(Vector2.up);
        }

        else
        {
            if (Gamepad.all[0].dpad.left.wasPressedThisFrame)
                ChangePosition(Vector2.left);
            
            else if (Gamepad.all[0].dpad.right.wasPressedThisFrame)
                ChangePosition(Vector2.right);
            
            else if (Gamepad.all[0].dpad.down.wasPressedThisFrame)
                ChangePosition(Vector2.down);
            
            else if (Gamepad.all[0].dpad.up.wasPressedThisFrame)
                ChangePosition(Vector2.up);
        }
    }

    void CheckJoystickInput(InputAction.CallbackContext context)
    {   
        if (useController)
        {
            gamepadAxis = context.ReadValue<Vector2>();

            if (gamepadAxis == Vector2.left)
                ChangePosition(Vector2.left);
                
            else if (gamepadAxis == Vector2.right)
                ChangePosition(Vector2.right);
                
            else if (gamepadAxis == Vector2.down)
                ChangePosition(Vector2.down);
                
            else if (gamepadAxis == Vector2.up)
                ChangePosition(Vector2.up);
        }
    }

    GameObject GetPelletAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject pellet = manager.boardObjects[tileX, tileY];

        if (pellet != null)
            return pellet;
        
        return null;
    }

    void EatPellet()
    {
        GameObject pellet = GetPelletAtPosition(transform.localPosition);

        if (pellet != null)
        {
            if (!pellet.GetComponent<Node>().eaten && !pellet.GetComponent<Node>().invisiblePellet && pellet.GetComponent<Node>().pelletType != Node.PelletType.SuperPellet)
            {
                pellet.GetComponent<SpriteRenderer>().enabled = false;
                pellet.GetComponent<Node>().eaten = true;
                manager.score += pellet.GetComponent<Node>().scoreValue;
                manager.eatenPellets++;
                manager.pelletFruitCounter++;
                manager.CheckToAppearFruit();
                manager.CheckForSirenChange();
                StartCoroutine(manager.ControllerRumble(.5f));
                source.PlayOneShot(_munch ? munchSFX[1] : munchSFX[0]);
                _munch = !_munch;
            }

            else if (!pellet.GetComponent<Node>().eaten && !pellet.GetComponent<Node>().invisiblePellet && pellet.GetComponent<Node>().pelletType == Node.PelletType.SuperPellet)
            {
                //Super Pellet
                pellet.GetComponent<SpriteRenderer>().enabled = false;
                pellet.GetComponent<Node>().eaten = true;
                manager.score += pellet.GetComponent<Node>().scoreValue;
                manager.eatenPellets++;
                manager.pelletFruitCounter++;
                source.PlayOneShot(superPelletEat);
                manager.CheckToAppearFruit();
                manager.CheckForSirenChange();
                manager.TriggerSuperPellet();
            }
        }
    }

    Transform GetPortal(Vector2 pos)
    {
        GameObject pellet = manager.boardObjects[(int)pos.x, (int)pos.y];

        if (pellet != null && pellet.GetComponent<Node>() != null)
        {
            if (pellet.GetComponent<Node>().isPortal)
            {
                return pellet.GetComponent<Node>().portalEnd;
            }
        }

        return null;
    }

    public void Restart()
    {
        canMove = true;
        animator.SetTrigger("reappear");
        transform.position = startingNode.transform.position;
        currentNode = startingNode;
        playerInputVector = orientation = nextDirection = currentNode.ConvertDirectionFromEnum(startingDirection);
        ChangePosition(playerInputVector);
    }

    void MovementLight(float incrementFactor)
    {
        if (!overrideAnimSpeed)
        {
            if (incrementFactor > 0f)
            {
                if (manager.currentAnimSpeed < 1f)
                {
                    manager.currentAnimSpeed += Time.deltaTime;
                }
            }
            
            else
            {
                if (manager.currentAnimSpeed > 0f)
                {
                    manager.currentAnimSpeed -= Time.deltaTime;

                    if (manager.currentAnimSpeed < 0f)
                        manager.currentAnimSpeed = 0f;
                }
            }
        }
    }

    float LengthFromNode(Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    void HandlePS4LightBar()
    {
        Vector2 _playerInputVector = playerInputVector;
        float incrementFactor = ((Mathf.Abs(_playerInputVector.x) / 2) + (Mathf.Abs(_playerInputVector.y) / 2));
        MovementLight(incrementFactor);
    }
}
