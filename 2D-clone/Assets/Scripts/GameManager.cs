using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using XInputDotNetPure;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum PlayerOneControls { Keyboard, Controller }
    [Header ("Game")]
    public PlayerOneControls playerOneControls;
    public enum PlayerTwoControls { Keyboard, Controller }
    public int controllerOneIndexToUse;
    public PlayerTwoControls playerTwoControls;
    public int controllerTwoIndexToUse;
    public enum GameMode { Classic, PVP2P, COOP2P, GhostVPlayer, TimeTrial }
    public GameMode currentGamemode;
    [Header ("Board")]
    public Node[] cornerNodes;
    public List<Node> allNodes;
    public static int boardWidth = 36, boardHeight = 36;
    public GameObject [,] boardObjects = new GameObject[boardWidth, boardHeight];
    public int score, eatenPellets, totalPellets, lastGhostScore = 200;
    public int p2Score, p2EatenPellets, p2LastGhostScore = 200;
    public float[] pelletDivision;
    public Maze mazeData;
    public Player pacMan;
    public Player pacMan2;
    public Enemy blinky, inky, pinky, clyde;
    public bool countTime;
    public enum MusicMode { Sirens, Custom }
    public MusicMode musicMode;
    public AudioClip customMusic;
    public DifficultySetting difficulty;
    public List<Fruit> roundFruits = new List<Fruit>();
    public int pelletsToAppearFruit;
    public int pelletFruitCounter;
    public int currentActIndex;
    public Transform fruitSpawn;
    public float act1T, act2T, act3T;
    [Header ("Audio")]
    public AudioClip[] sirens;
    public AudioClip superPelletMusic;
    public AudioClip consumedGhostMusic;
    public AudioClip pacManDeath;
    public AudioClip introMusic;
    public AudioClip winMusic;
    public AudioSource musicSource;
    public int consumedGhosts;
    public int ghostToConsume;
    public bool ateAllFruits;
    [Header ("UI")]
    public Animator[] fruits;
    public TMP_Text currentScore;
    public TMP_Text hiScore;
    public TMP_Text currentTime;
    public TMP_Text bestTime;
    public TMP_Text currentLives;
    public TMP_Text restartReadyText, restartPlayerText, difficultyName,
    currentPelletsText, totalPelletsText, act1TText, act2TText, act3TText,
    totalTimeText, totalScoreText, actDeathText;
    public string ghostKilledName;
    public Animator gameOverPanel;
    public GameObject[] killedGhosts;
    [Header ("2 Player PVP UI")]
    public Slider playerOneProgress;
    public Slider playerTwoProgress;
    public GameObject playerOneCrown, playerTwoCrown;
    public TMP_Text playerOneCurrentPellets, playerTwoCurrentPellets;
    public TMP_Text[] totalLevelPelletsText;
    public Animator[] playerOneFruits, playerTwoFruits;

    [Header ("Controllers")]
    public Player player;
    public Color PS4BarLightColor;
    public float currentAnimSpeed;
    public float superPelletTime;
    public Gamepad[] connectedGamepads;
    public List<DualShock4GamepadHID> ps4Gamepads = new List<DualShock4GamepadHID>();
    public Gamepad playerOneGamepad, playerTwoGamepad;

    XInputDotNetPure.PlayerIndex playerOneXIndex;
    XInputDotNetPure.PlayerIndex playerTwoXIndex;

    public GameObject createdFruit;
    public int fruitIndex;
    int totalLevelPellets;
    int combinedPellets;
    bool winAct;
    bool startDeath;
    bool inSuperPellet;
    bool playGameOverMusic;
    bool _playedGO;
    bool on2PLoose;
    float timeAct1, timeAct2, timeAct3;
    float controllerRumbleTime, currentControllerRumble;
    Vector2 _rumbleForce;
    bool startRumble;
    int currentSirenIndex;
    float superPelletTimer;
    float timeSpent;
    Enemy[] allGhosts = new Enemy[4];
    Animator animator;
    Transform lastPellet;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (Gamepad.all.Count > 0)
            SetControllers();
        
        AssignPlayersControls();

        for (int i = 0; i < cornerNodes.Length; i++)
            if (!cornerNodes[i].invisiblePellet)
                totalPellets++;
        
        for (int i = 0; i < allNodes.Count; i++)
            totalPellets++;

        SetGameBoard();

        allGhosts[0] = blinky;
        allGhosts[1] = pinky;
        allGhosts[2] = inky;
        allGhosts[3] = clyde;

        totalPelletsText.text = totalPellets.ToString("D3");

        for (int i = 0; i < pelletDivision.Length; i++)
            pelletDivision[i] = (totalPellets / 6f) * (i + 1);
        
        LoadMaze();
        LoadDifficulty();

        if (currentGamemode == GameMode.PVP2P)
            SetUpPVP2P();
        
        StartCoroutine(StartCutscene(introMusic.length));
    }

    void Update()
    {
        if (inSuperPellet)
            HandleSuperPellet();

        HandleUI();
        
        if (countTime)
            HandleTimer();

        CheckForWinGame();
        PS4LightBarManager();

        if (currentGamemode == GameMode.PVP2P)
            CheckForPVP2PSpecifics();

        if ((currentGamemode != GameMode.Classic || currentGamemode != GameMode.TimeTrial) && on2PLoose)
        {
            pacMan.canMove = false;
            pacMan2.canMove = false;
        }
        
        if (startRumble)
            RumbleController();

        if (playGameOverMusic)
            GameOverMusic();

        if (Keyboard.current.kKey.wasPressedThisFrame)
            InstaWin();
    }

    void InstaWin()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            if (!allNodes[i].eaten && !allNodes[i].invisiblePellet)
            {
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
                allNodes[i].eaten = true;
            }
        }

        foreach (Animator a in fruits)
            a.SetTrigger("collected");

        winAct = true;

        currentActIndex++;

        if (currentActIndex > mazeData.mazeActs.Length - 1)
            StartCoroutine(WinWin());
        else
            StartCoroutine(ProcessWinActAnimation());
    }

    void AssignPlayersControls()
    {
        if (currentGamemode != GameMode.Classic && currentGamemode != GameMode.TimeTrial)
        {
            if (playerOneControls == PlayerOneControls.Controller && playerTwoControls == PlayerTwoControls.Controller)
            {
                if (!CheckForEnoughControllers(2))
                {   
                    if (!CheckForEnoughControllers(1))
                    {
                        Debug.Log("No controllers connected, assigning both players to keyboard");
                        playerOneControls = PlayerOneControls.Keyboard;
                        playerTwoControls = PlayerTwoControls.Keyboard;
                        pacMan.playerController = pacMan2.playerController = Player.PlayerController.Keyboard;
                    }

                    else
                    {
                        Debug.Log("Not enough controllers for 2 player mode, assignign controller to player 1");
                        playerTwoControls = PlayerTwoControls.Keyboard;
                        playerOneGamepad = Gamepad.all[0];
                        pacMan.playerController = Player.PlayerController.Controller;
                        pacMan2.playerController = Player.PlayerController.Keyboard;
                    }
                }

                else
                {
                    Debug.Log("There are enough controllers for 2 player mode");
                    playerOneGamepad = Gamepad.all[controllerOneIndexToUse];
                    playerTwoGamepad = Gamepad.all[controllerTwoIndexToUse];
                    pacMan.playerController = pacMan2.playerController = Player.PlayerController.Controller;
                }
            }

            else if (playerOneControls == PlayerOneControls.Controller && playerTwoControls == PlayerTwoControls.Keyboard)
            {
                if (CheckForEnoughControllers(1))
                {
                    Debug.Log("There are no controllers connected, assigning keyboard to player 1");
                    playerOneControls = PlayerOneControls.Keyboard;
                }

                else
                {
                    Debug.Log("There is one controller connected");

                    if (controllerOneIndexToUse > Gamepad.all.Count - 1)
                    {
                        controllerOneIndexToUse = 0;
                        playerOneGamepad = Gamepad.all[controllerOneIndexToUse];
                    }

                    else
                        playerOneGamepad = Gamepad.all[controllerOneIndexToUse];
                }
            }

            else if (playerOneControls == PlayerOneControls.Keyboard && playerTwoControls == PlayerTwoControls.Controller)
            {
                if (!CheckForEnoughControllers(1))
                {
                    Debug.Log("There are not controllers connected, assigning keyboard to player 2");
                    playerTwoControls = PlayerTwoControls.Keyboard;
                }

                else
                {
                    Debug.Log("There is one controller connected");

                    if (controllerTwoIndexToUse > Gamepad.all.Count - 1)
                    {
                        controllerTwoIndexToUse = 0;
                        playerTwoGamepad = Gamepad.all[controllerTwoIndexToUse];
                    }

                    else
                        playerTwoGamepad = Gamepad.all[controllerTwoIndexToUse];
                }
            }
        }

        else
        {
            if (playerOneControls == PlayerOneControls.Controller)
            {
                if (!CheckForEnoughControllers(1))
                {
                    Debug.Log("There are no controllers connected, assigning keyboard to player 1");
                    playerOneControls = PlayerOneControls.Keyboard;
                    pacMan.playerController = Player.PlayerController.Keyboard;
                }

                else
                {
                    Debug.Log("There is one controller connected");

                    if (controllerOneIndexToUse > Gamepad.all.Count - 1)
                    {
                        controllerOneIndexToUse = 0;
                        playerOneGamepad = Gamepad.all[controllerOneIndexToUse];
                    }

                    else
                        playerOneGamepad = Gamepad.all[controllerOneIndexToUse];

                    pacMan.playerController = Player.PlayerController.Controller;
                }
            }
        }
    }

    void SetControllers()
    {
        connectedGamepads = new Gamepad[Gamepad.all.Count];

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            connectedGamepads[i] = Gamepad.all[i];

            if (Gamepad.all[i] is DualShock4GamepadHID)
                ps4Gamepads.Add((DualShock4GamepadHID)Gamepad.all[i]);
        }
    }

    void SetUpPVP2P()
    {
        totalLevelPellets = totalPellets * mazeData.mazeActs.Length;
        playerOneProgress.maxValue = totalLevelPellets;
        playerTwoProgress.maxValue = totalLevelPellets;

        for (int i = 0; i < playerOneFruits.Length; i++)
        {
            foreach (Transform c in playerOneFruits[i].transform)
            {
                if (c.GetComponent<SpriteRenderer>() != null)
                    c.GetComponent<SpriteRenderer>().sprite = roundFruits[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            }
        }

        for (int i = 0; i < playerTwoFruits.Length; i++)
        {
            foreach (Transform c in playerTwoFruits[i].transform)
            {
                if (c.GetComponent<SpriteRenderer>() != null)
                    c.GetComponent<SpriteRenderer>().sprite = roundFruits[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            }
        }
    }

    void CheckForPVP2PSpecifics()
    {
        if (score > p2Score && !playerOneCrown.activeSelf)
        {
            playerOneCrown.SetActive(true);
            playerTwoCrown.SetActive(false);
        }
        
        else if (p2Score > score && !playerTwoCrown.activeSelf)
        {
            playerOneCrown.SetActive(false);
            playerTwoCrown.SetActive(true);
        }

        else if ((score == p2Score) && (playerTwoCrown.activeSelf || playerOneCrown.activeSelf))
        {
            playerOneCrown.SetActive(false);
            playerTwoCrown.SetActive(false);
        }

        playerOneCurrentPellets.text = eatenPellets.ToString("D4");
        playerTwoCurrentPellets.text = p2EatenPellets.ToString("D4");

        playerOneProgress.value = eatenPellets;
        playerTwoProgress.value = p2EatenPellets;

        combinedPellets = eatenPellets + p2EatenPellets;

        foreach (TMP_Text t in totalLevelPelletsText)
            t.text = "/ " + totalLevelPellets.ToString("D4");
    }

    public bool CheckForEnoughControllers(int controllerAmount)
    {
        Debug.Log(Gamepad.all.Count + " " + controllerAmount);

        if (Gamepad.all.Count >= controllerAmount)
            return true;

        return false;
    }

    void HandleUI()
    {
        currentScore.text = score.ToString("D6");
        currentLives.text = pacMan.pacManLives.ToString();
        currentPelletsText.text = eatenPellets.ToString("D3");
        currentTime.text = TimeSpan.FromSeconds(timeSpent).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(timeSpent).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(timeSpent).Milliseconds.ToString();
    }

    void GameOverMusic()
    {   
        musicSource.clip = introMusic;
        musicSource.loop = false;

        if (!_playedGO)
        {
            musicSource.Play();
            _playedGO = true;
        }

        if (musicSource.pitch > 0)
            musicSource.pitch -= Time.deltaTime * 0.12f;
    }

    void LoadMaze()
    {
        if (mazeData != null)
        {
            roundFruits = mazeData.mazeActs[currentActIndex].fruitsToAppear;
            pelletsToAppearFruit = mazeData.pelletsToAppearFruit;

            if (currentGamemode == GameMode.Classic)
            {
                for (int i = 0; i < fruits.Length; i++)
                {
                    foreach (Transform c in fruits[i].transform)
                    {
                        if (c.GetComponent<SpriteRenderer>() != null)
                        {
                            c.GetComponent<SpriteRenderer>().sprite = roundFruits[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                        }
                    }
                }
            }
        }
    }

    void LoadDifficulty()
    {
        if (difficulty != null)
        {
            difficultyName.text = difficulty.difficultyName;
            superPelletTime = difficulty.superPelletMaxDuration;
            pacMan.moveSpeed = difficulty.pacManSpeed;
            pacMan.pacManLives = difficulty.pacManStartingLives;

            if (currentGamemode != GameMode.Classic || currentGamemode != GameMode.TimeTrial || currentGamemode != GameMode.GhostVPlayer)
            {
                pacMan2.moveSpeed = difficulty.pacManSpeed;
                pacMan2.pacManLives = difficulty.pacManStartingLives;
            }

            customMusic = difficulty.customMusic;
            musicMode = difficulty.difficultyMusicMode;

            foreach (Fruit f in roundFruits)
                f.lifeTime = difficulty.fruitLifeTime;

            for (int i = 0; i < difficulty.ghostConfigs.Length; i++)
            {   
                Enemy tempGhost = null;

                switch(difficulty.ghostConfigs[i].ghostName)
                {
                    case "blinky":
                        tempGhost = blinky;
                    break;

                    case "inky":
                        tempGhost = inky;
                    break;

                    case "pinky":
                        tempGhost = pinky;
                    break;

                    case "clyde":
                        tempGhost = clyde;
                    break;
                }

                tempGhost.timeToReleaseGhost = difficulty.ghostConfigs[i].timeToRelease;
                tempGhost.moveSpeed = difficulty.ghostConfigs[i].ghostMoveSpeed;
                tempGhost.consumedMoveSpeed = difficulty.ghostConfigs[i].ghostConsumedSpeed;
                tempGhost.maxModeChangeIteration = difficulty.maxIterationModes;
                tempGhost.scatterModeTimes = difficulty.scatterModeTimes;
                tempGhost.chaseModeTimes = difficulty.chaseModeTimes;
            }
        }
    }

    IEnumerator StartCutscene(float delay)
    {
        musicSource.PlayOneShot(introMusic);
        pacMan.canMove = false;

        if (currentGamemode != GameMode.Classic || currentGamemode != GameMode.TimeTrial || currentGamemode != GameMode.GhostVPlayer)
            pacMan2.canMove = false;

        blinky.canMove = pinky.canMove = inky.canMove = clyde.canMove = false;
        pacMan.GetComponent<Animator>().SetBool("moving", false);

        if (currentGamemode != GameMode.Classic || currentGamemode != GameMode.TimeTrial || currentGamemode != GameMode.GhostVPlayer)
            pacMan2.GetComponent<Animator>().SetBool("moving", false);

        yield return new WaitForSeconds(delay);

        countTime = true;
        pacMan.canMove = true;

        if (currentGamemode != GameMode.Classic || currentGamemode != GameMode.TimeTrial || currentGamemode != GameMode.GhostVPlayer)
            pacMan2.canMove = true;

        blinky.canMove = pinky.canMove = inky.canMove = clyde.canMove = true;

        if (musicMode != MusicMode.Custom)
        {
            musicSource.clip = sirens[0];
            musicSource.Play();
        }

        else
        {
            musicSource.clip = customMusic;
            musicSource.Play();
        }

        musicSource.loop = true;
    }

    void HandleTimer()
    {
        timeSpent += Time.deltaTime;
    }

    public void CheckToAppearFruit()
    {
        if (pelletFruitCounter >= pelletsToAppearFruit && !ateAllFruits)
        {
            if (createdFruit == null)
            {
                createdFruit = Instantiate(roundFruits[fruitIndex].gameObject, fruitSpawn.position, Quaternion.identity);
                createdFruit.GetComponent<Fruit>().lifeTime = difficulty.fruitLifeTime;
                createdFruit.GetComponent<Fruit>().DestroyFruit();
            }

            pelletFruitCounter = 0;
        }   
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
        Node[] pellets = GameObject.FindObjectsOfType<Node>();

        foreach (Node o in pellets)
        {
            if (o.GetComponent<Node>() != null)
            {
                Vector2 pos = o.transform.position;
                boardObjects[Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.y)] = o.gameObject;
            }
        }
    }

    public void CheckForSirenChange()
    {   
        if (musicMode != MusicMode.Custom)
        {
            for (int i = 0; i < pelletDivision.Length; i++)
            {   
                if (currentGamemode == GameMode.Classic)
                {
                    if (pelletDivision[i] > 0 && eatenPellets >= pelletDivision[i])
                    {   
                        if (!inSuperPellet)
                        {
                            musicSource.clip = sirens[i];
                            musicSource.Play();
                        }

                        pelletDivision[i] = -1;
                        currentSirenIndex = i;
                        break;
                    }
                }

                else if (currentGamemode == GameMode.PVP2P || currentGamemode == GameMode.COOP2P)
                {
                    if (pelletDivision[i] > 0 && combinedPellets >= pelletDivision[i])
                    {
                        if (!inSuperPellet)
                        {
                            musicSource.clip = sirens[i];
                            musicSource.Play();
                        }

                        pelletDivision[i] = -1;
                        currentSirenIndex = i;
                        break;
                    }
                }
            }
        }
    }

    public void RestartGame()
    {
        pacMan.Restart();

        if (pacMan2 != null)
            pacMan2.Restart();

        blinky.Restart();
        inky.Restart();
        pinky.Restart();
        clyde.Restart();

        if (inSuperPellet)
        {
            superPelletTimer = 0f;
            consumedGhosts = 0;
            inSuperPellet = false;
        }

        pelletFruitCounter = 0;
        lastGhostScore = 200;
        p2LastGhostScore = 200;

        if (musicMode != MusicMode.Custom) musicSource.clip = sirens[currentSirenIndex];
        else musicSource.clip = customMusic;

        musicSource.Play();

        startDeath = false;
    }

    public void TriggerSuperPellet()
    {
        player.overrideAnimSpeed = true;
        currentAnimSpeed = 1f;

        if (blinky.ghostMode != Enemy.GhostMode.Frightened && blinky.ghostMode != Enemy.GhostMode.Consumed)
            blinky.StartFrightenedMode();

        if (inky.ghostMode != Enemy.GhostMode.Frightened && inky.ghostMode != Enemy.GhostMode.Consumed)
            inky.StartFrightenedMode();

        if (pinky.ghostMode != Enemy.GhostMode.Frightened && pinky.ghostMode != Enemy.GhostMode.Consumed)
            pinky.StartFrightenedMode();

        if (clyde.ghostMode != Enemy.GhostMode.Frightened && clyde.ghostMode != Enemy.GhostMode.Consumed)
            clyde.StartFrightenedMode();

        if (!inSuperPellet)
        {
            inSuperPellet = true;

            if (musicMode != MusicMode.Custom)
            {
                musicSource.clip = superPelletMusic;
                musicSource.Play();
            }

            animator.SetTrigger("superpelletlight");
            ghostToConsume = 4;
        }
        
        else
        {
            ghostToConsume = 4;
            lastGhostScore = 200;

            if (!animator.GetBool("superpelletlightend"))
                superPelletTimer = 0f;
            
            else
            {
                animator.SetBool("superpelletlightend", false);
                animator.SetTrigger("superpelletlight");

                if (blinky.ghostMode == Enemy.GhostMode.Frightened)
                    blinky.animator.runtimeAnimatorController = blinky.frightStart;
                
                if (inky.ghostMode == Enemy.GhostMode.Frightened)
                    inky.animator.runtimeAnimatorController = inky.frightStart;

                if (pinky.ghostMode == Enemy.GhostMode.Frightened)
                    pinky.animator.runtimeAnimatorController = pinky.frightStart;

                if (clyde.ghostMode == Enemy.GhostMode.Frightened)
                    clyde.animator.runtimeAnimatorController = clyde.frightStart;

                superPelletTimer = 0f;
            }
        }
    }

    void CheckForWinGame()
    {   
        if (currentGamemode == GameMode.Classic)
        {
            if (eatenPellets >= totalPellets && !winAct)
            {
                winAct = true;
                countTime = false;

                switch (currentActIndex)
                {
                    case 0:
                        act1T = timeAct1 = timeSpent;

                        act1TText.text = TimeSpan.FromSeconds(act1T).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(act1T).Seconds.ToString("D2") + ":" +
                            TimeSpan.FromSeconds(act1T).Milliseconds.ToString();
                    break;

                    case 1:
                        act2T = timeAct2 = timeSpent;

                        act2TText.text = TimeSpan.FromSeconds(act2T).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(act2T).Seconds.ToString("D2") + ":" +
                            TimeSpan.FromSeconds(act2T).Milliseconds.ToString();
                    break;

                    case 2:
                        act3T = timeAct3 = timeSpent;

                        act3TText.text = TimeSpan.FromSeconds(act3T).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(act3T).Seconds.ToString("D2") + ":" +
                            TimeSpan.FromSeconds(act3T).Milliseconds.ToString();
                    break;
                }

                timeSpent = 0f;

                currentActIndex++;

                if (currentActIndex > mazeData.mazeActs.Length - 1)
                    StartCoroutine(WinWin());

                else
                    StartCoroutine(ProcessWinActAnimation());
            }
        }

        else if (currentGamemode == GameMode.PVP2P)
        {
            if (combinedPellets >= totalPellets && !winAct)
            {
                winAct = true;

                currentActIndex++;

                if (currentActIndex > mazeData.mazeActs.Length - 1)
                    StartCoroutine(PVP2PWinWin());
                
                else
                    StartCoroutine(PVP2PWinActAnimation());
            }
        }
    }

    IEnumerator PVP2PWinWin()
    {
        yield return null;
    }

    IEnumerator PVP2PWinActAnimation()
    {
        pacMan.canMove = pacMan2.canMove = blinky.canMove = inky.canMove = pinky.canMove = clyde.canMove = false;
        pacMan.GetComponent<Animator>().SetBool("moving", false);
        pacMan2.GetComponent<Animator>().SetBool("moving", false);
        musicSource.Stop();
        fruitIndex = 0;
        ateAllFruits = false;
        lastGhostScore = 200;
        p2LastGhostScore = 200;
        currentSirenIndex = 0;
        pelletFruitCounter = 0;

        if (createdFruit != null)
            createdFruit.GetComponent<Fruit>().InstaDestroyFruit();

        if (inSuperPellet)
        {
            superPelletTimer = 0f;
            inSuperPellet = false;
        }

        yield return new WaitForSeconds(2f);

        blinky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        inky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        pinky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        clyde.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        pacMan2.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        musicSource.Stop();
        musicSource.PlayOneShot(winMusic);

        for (int i = 0; i < allNodes.Count; i++)
        {
            if (!allNodes[i].invisiblePellet && allNodes[i].eaten)
            {
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;
                allNodes[i].GetComponent<Node>().eaten = false;
                yield return new WaitForSeconds(0.009f);
            }
        }

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(3.5f);

        restartReadyText.gameObject.SetActive(true);
        restartPlayerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        restartReadyText.gameObject.SetActive(false);
        restartPlayerText.gameObject.SetActive(false);

        //animator.SetTrigger("pacwinend");

        NextAct();
    }

    public void NextAct()
    {
        RestartGame();

        winAct = false;
        countTime = true;
        animator.ResetTrigger("pacwinend");
        animator.ResetTrigger("pacwinstart");
        animator.ResetTrigger("pacdeathend");
        animator.ResetTrigger("pacdeathstart");

        pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

        if (pacMan2 != null)
            pacMan2.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

        roundFruits = mazeData.mazeActs[currentActIndex].fruitsToAppear;
        pelletsToAppearFruit = mazeData.pelletsToAppearFruit;

        if (currentGamemode == GameMode.Classic)
        {
            for (int i = 0; i < fruits.Length; i++)
            {
                foreach (Transform c in fruits[i].transform)
                {
                    if (c.GetComponent<SpriteRenderer>() != null)
                    {
                        c.GetComponent<SpriteRenderer>().sprite = roundFruits[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                    }
                }
            }

            foreach (Animator a in fruits)
                a.SetTrigger("lock");
        }

        else if (currentGamemode == GameMode.PVP2P)
        {
            for (int i = 0; i < playerOneFruits.Length; i++)
            {
                foreach (Transform c in playerOneFruits[i].transform)
                {
                    if (c.GetComponent<SpriteRenderer>() != null)
                        c.GetComponent<SpriteRenderer>().sprite = roundFruits[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                }
            }

            foreach (Animator a in playerOneFruits)
                a.SetTrigger("lock");

            for (int i = 0; i < playerTwoFruits.Length; i++)
            {
                foreach (Transform c in playerTwoFruits[i].transform)
                {
                    if (c.GetComponent<SpriteRenderer>() != null)
                        c.GetComponent<SpriteRenderer>().sprite = roundFruits[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                }
            }

            foreach (Animator a in playerTwoFruits)
                a.SetTrigger("lock");
        }
    }

    public Transform GetPreferredTarget()
    {
        if (score > p2Score)
            return pacMan.transform;
        
        else if (p2Score > score)
            return pacMan2.transform;
        
        else if (p2Score == score)
            return pacMan.transform;
        
        return null;
    }

    IEnumerator ProcessWinActAnimation()
    {
        pacMan.canMove = blinky.canMove = inky.canMove = pinky.canMove = clyde.canMove = false;
        pacMan.GetComponent<Animator>().SetBool("moving", false);
        musicSource.Stop();
        fruitIndex = 0;
        ateAllFruits = false;
        eatenPellets = 0;
        lastGhostScore = 200;
        p2LastGhostScore = 200;
        currentSirenIndex = 0;
        pelletFruitCounter = 0;
        
        if (createdFruit != null)
            createdFruit.GetComponent<Fruit>().InstaDestroyFruit();

        if (inSuperPellet)
        {
            superPelletTimer = 0f;
            inSuperPellet = false;
        }

        yield return new WaitForSeconds(2f);

        blinky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        inky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        pinky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        clyde.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        animator.SetTrigger("pacwinstart");

        musicSource.Stop();
        musicSource.PlayOneShot(winMusic);

        for (int i = 0; i < allNodes.Count; i++)
        {
            if (!allNodes[i].invisiblePellet && allNodes[i].eaten)
            {
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;
                allNodes[i].GetComponent<Node>().eaten = false;
                yield return new WaitForSeconds(0.009f);
            }
        }

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.09f);

        for (int i = 0; i < allNodes.Count; i++)
            if (!allNodes[i].invisiblePellet)
                allNodes[i].GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(3.5f);

        restartReadyText.gameObject.SetActive(true);
        restartPlayerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        restartReadyText.gameObject.SetActive(false);
        restartPlayerText.gameObject.SetActive(false);

        animator.SetTrigger("pacwinend");

        NextAct();
    }

    IEnumerator WinWin()
    {
        yield return null;
    }

    public void StartDeath(Player pacManTo = null)
    {
        if (!startDeath) startDeath = true;

        if (pacManTo == null)
        {
            if (pacMan.pacManLives > 0)
            {
                pacMan.pacManLives--;

                if (pacMan.pacManLives <= 0)
                {
                    countTime = false;

                    switch (currentActIndex)
                    {
                        case 0:
                            timeAct1 = timeSpent;
                        break;

                        case 1:
                            timeAct2 = timeSpent;
                        break;

                        case 2:
                            timeAct3 = timeSpent;
                        break;
                    }
                }
            }

            else
            {
                countTime = false;

                switch (currentActIndex)
                {
                    case 0:
                        timeAct1 = timeSpent;
                    break;

                    case 1:
                        timeAct2 = timeSpent;
                    break;

                    case 2:
                        timeAct3 = timeSpent;
                    break;
                }
            }
        }

        pacMan.canMove = blinky.canMove = inky.canMove = pinky.canMove = clyde.canMove = false;
        pacMan.GetComponent<Animator>().SetBool("moving", false);

        if (currentGamemode != GameMode.Classic || currentGamemode != GameMode.TimeTrial || currentGamemode != GameMode.GhostVPlayer)
        {
            pacMan2.canMove = false;
            pacMan2.GetComponent<Animator>().SetBool("moving", false);
        }

        musicSource.Stop();

        if (inSuperPellet)
        {
            superPelletTimer = 0f;
            inSuperPellet = false;
        }

        if (currentGamemode != GameMode.Classic && currentGamemode != GameMode.TimeTrial && currentGamemode != GameMode.GhostVPlayer)
        {
            on2PLoose = true;
            StartCoroutine(ProcessAfterDeath2Player(2f, pacManTo));
        }

        else if (currentGamemode == GameMode.PVP2P || currentGamemode == GameMode.COOP2P)
            StartCoroutine(ProcessAfterDeath(2f));
    }

    IEnumerator ProcessAfterDeath2Player(float delay, Player pacManTo)
    {
        yield return new WaitForSeconds(2f);

        pacManTo.pacManLives--;

        if (pacManTo.pacManLives <= 0)
        {
            //2 Player Win
        }

        else
        {
            musicSource.clip = sirens[currentSirenIndex];
            musicSource.Play();

            if (pacManTo == pacMan)
            {
                pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX =
                pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
                pacMan.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                pacMan.GetComponent<Animator>().SetTrigger("death");
                pacMan.GetComponent<AudioSource>().PlayOneShot(pacManDeath);
                pacMan.invul = true;
                StartCoroutine(ReAppearPacMan(2f, pacMan));
            }

            else
                pacMan.canMove = true;

            if (pacManTo == pacMan2)
            {
                pacMan2.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX =
                pacMan2.transform.GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
                pacMan2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                pacMan2.GetComponent<Animator>().SetTrigger("death");
                pacMan2.GetComponent<AudioSource>().PlayOneShot(pacManDeath);
                pacMan2.invul = true;
                StartCoroutine(ReAppearPacMan(2f, pacMan2));
            }

            else
                pacMan2.canMove = true;

            on2PLoose = false;
            blinky.canMove = pinky.canMove = inky.canMove = clyde.canMove = true;
        }
    }

    IEnumerator ReAppearPacMan(float delay, Player pacManTo)
    {
        yield return new WaitForSeconds(delay);
        pacManTo.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        pacManTo.RestartWithInvul();
    }

    IEnumerator ProcessAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        blinky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        inky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        pinky.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled =
        clyde.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        StartCoroutine(ProcessDeathAnimation(1.9f));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        Debug.Log("here3");

        if (createdFruit != null)
            createdFruit.GetComponent<Fruit>().InstaDestroyFruit();

        pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX =
        pacMan.transform.GetChild(0).GetComponent<SpriteRenderer>().flipY = false;

        pacMan.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        pacMan.GetComponent<Animator>().SetTrigger("death");
        animator.SetTrigger("pacdeathstart");

        musicSource.PlayOneShot(pacManDeath);
        
        yield return new WaitForSeconds(delay);

        countTime = false;

        if (pacMan.pacManLives > 0)
            StartCoroutine(ProcessRestart(2f));
        
        else if (pacMan.pacManLives <= 0)
            StartCoroutine(ProcessGameOver(2f));
    }
    
    IEnumerator ProcessGameOver(float delay)
    {
        playGameOverMusic = true;
        restartReadyText.gameObject.SetActive(true);

        if (pacMan.pacManLives <= 0)
        {
            restartReadyText.text = "GAME OVER";
            restartReadyText.color = Color.red;
        }

        GameOverMenu();

        yield return new WaitForSeconds(delay);
    }

    IEnumerator ProcessRestart(float delay)
    {
        restartReadyText.gameObject.SetActive(true);
        restartPlayerText.gameObject.SetActive(true);

        if (pacMan.pacManLives <= 0)
        {
            restartReadyText.text = "LAST LIFE!";
            restartPlayerText.text = "WARNING!";
        }

        yield return new WaitForSeconds(delay);

        countTime = true;
        restartReadyText.gameObject.SetActive(false);
        restartPlayerText.gameObject.SetActive(false);

        animator.SetTrigger("pacdeathend");
        RestartGame();
    }

    void GameOverMenu()
    {
        totalScoreText.text = score.ToString("D6");
        actDeathText.text = (currentActIndex + 1).ToString();
        float _totTime = timeAct1 + timeAct2 + timeAct3;
        totalTimeText.text = TimeSpan.FromSeconds(_totTime).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(_totTime).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(_totTime).Milliseconds.ToString();
        
        switch (ghostKilledName)
        {
            case "blinky":
                killedGhosts[0].SetActive(true);
            break;

            case "pinky":
                killedGhosts[1].SetActive(true);
            break;

            case "inky":
                killedGhosts[2].SetActive(true);
            break;

            case "clyde":
                killedGhosts[3].SetActive(true);
            break;
        }

        gameOverPanel.SetTrigger("ui_gameover");
    }

    void HandleSuperPellet()
    {
        if (superPelletTimer >= superPelletTime - (superPelletTime / 3f))
        {
            animator.SetBool("superpelletlightend", true);

            if (blinky.ghostMode == Enemy.GhostMode.Frightened)
                blinky.animator.runtimeAnimatorController = blinky.frightEnd;
            
            if (inky.ghostMode == Enemy.GhostMode.Frightened)
                inky.animator.runtimeAnimatorController = inky.frightEnd;

            if (pinky.ghostMode == Enemy.GhostMode.Frightened)
                pinky.animator.runtimeAnimatorController = pinky.frightEnd;

            if (clyde.ghostMode == Enemy.GhostMode.Frightened)
                clyde.animator.runtimeAnimatorController = clyde.frightEnd;
        }

        if (superPelletTimer <= superPelletTime)
        {
            superPelletTimer += Time.deltaTime;

            if (superPelletTimer >= superPelletTime || ghostToConsume <= 0)
            {
                animator.SetBool("superpelletlightend", false);
                inSuperPellet = false;
                player.overrideAnimSpeed = false;
                superPelletTimer = 0f;
                ghostToConsume = 4;
                lastGhostScore = 200;
                p2LastGhostScore = 200;
                
                if (blinky.ghostMode == Enemy.GhostMode.Frightened)
                    blinky.StopFrightenedMode();
                
                if (inky.ghostMode == Enemy.GhostMode.Frightened)
                    inky.StopFrightenedMode();

                if (pinky.ghostMode == Enemy.GhostMode.Frightened)
                    pinky.StopFrightenedMode();

                if (clyde.ghostMode == Enemy.GhostMode.Frightened)
                    clyde.StopFrightenedMode();

                if (musicMode != MusicMode.Custom)
                {
                    musicSource.clip = sirens[currentSirenIndex];
                    musicSource.Play();
                }
            }
        }
    }

    public void CheckForConsumedGhosts(bool consumed)
    {   
        if (musicMode != MusicMode.Custom)
        {
            if (consumed)
            {
                if (musicSource.clip != consumedGhostMusic)
                {
                    musicSource.clip = consumedGhostMusic;
                    musicSource.Play();
                }
            }

            else
            {
                if (superPelletTimer >= superPelletTime || !inSuperPellet)
                {
                    if (musicSource.clip != sirens[currentSirenIndex])
                    {
                        musicSource.clip = sirens[currentSirenIndex];
                        musicSource.Play();
                    }
                }

                else if (superPelletTimer < superPelletTime)
                {
                    if (musicSource.clip != superPelletMusic)
                    {
                        musicSource.clip = superPelletMusic;
                        musicSource.Play();
                    }
                }
            }
        }
    }

    public void TriggerControllerRumble(float time, Vector2 rumbleForce)
    {
        controllerRumbleTime = time;
        _rumbleForce = rumbleForce;
        startRumble = true;
    }

    void RumbleController()
    {   
        if (Gamepad.all.Count > 0)
        {
            if (currentControllerRumble < controllerRumbleTime)
            {
                foreach (Gamepad g in connectedGamepads)
                {
                    if (g is DualShock4GamepadHID || g is DualShock3GamepadHID)
                    {
                        g.SetMotorSpeeds(_rumbleForce.x, _rumbleForce.y);
                    }
                }

                currentControllerRumble += Time.deltaTime;
            }

            else
                startRumble = false;
        }
    }

    public void ReloadGame()
    {
        gameOverPanel.SetTrigger("ui_option");
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    IEnumerator LoadScene(int sceneName)
    {
        yield return new WaitForSeconds(1.5f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            yield return null;
        }
    }

    void PS4LightBarManager()
    {   
        if (ps4Gamepads.Count > 0)
        {
            animator.speed = currentAnimSpeed;

            foreach (DualShock4GamepadHID ps4 in ps4Gamepads)
            {
                if (ps4 != null)
                    ps4.SetLightBarColor(PS4BarLightColor);
            }
        }
    }
}
