using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class GameManager : MonoBehaviour
{
    [Header ("Board")]
    public Node[] cornerNodes;
    public List<Node> allNodes;
    public static int boardWidth = 36, boardHeight = 36;
    public GameObject [,] boardObjects = new GameObject[boardWidth, boardHeight];
    public Maze mazeData;

    [Header ("PS4 Animation")]
    public Player player;
    public Color PS4BarLightColor;
    public float currentAnimSpeed;
    public float superPelletTime;

    Animator animator;
    DualShockGamepad ps4Gamepad;
    Transform lastPellet;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (Gamepad.all.Count > 0)
            ps4Gamepad = (DualShockGamepad)Gamepad.all[0];
        
        //mazeData.pelletsParent.transform.parent = null;

        SetGameBoard();
    }

    void Update()
    {
        PS4LightBarManager();
    }

    void SetGameBoard()
    {
        foreach (Node n in cornerNodes)
            allNodes.Add(n);

        for (int i = 0; i < cornerNodes.Length; i++)
        {
            if (cornerNodes[i].pelletType == Node.PelletType.CornerPellet && cornerNodes[i].gameObject.activeSelf)
                cornerNodes[i].GetNeighbours();
        }

        GetPelletsCoords();
    }

    void GetPelletsCoords()
    {
        GameObject[] pellets = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject o in pellets)
        {
            Vector2 pos = o.transform.position;

            if (o.GetComponent<Node>() != null)
                boardObjects[Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.y)] = o;
        }
    }

    public void TriggerSuperPellet(InputAction.CallbackContext context)
    {
        player.overrideAnimSpeed = true;
        currentAnimSpeed = 1f;
        StartCoroutine(DisableSuperPellet());
    }

    public IEnumerator DisableSuperPellet()
    {
        animator.SetTrigger("superpelletlight");
        yield return new WaitForSeconds(superPelletTime - (superPelletTime / 3f));
        animator.SetBool("superpelletlightend", true);
        yield return new WaitForSeconds(superPelletTime / 3f);
        animator.SetBool("superpelletlightend", false);
        player.overrideAnimSpeed = false;
    }

    void PS4LightBarManager()
    {
        animator.speed = currentAnimSpeed;

        if (ps4Gamepad != null)
            ps4Gamepad.SetLightBarColor(PS4BarLightColor);
    }
}
