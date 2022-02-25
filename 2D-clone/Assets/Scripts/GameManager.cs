using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header ("Board")]
    public Node[] cornerNodes;
    public List<Node> allNodes;
    public static int boardWidth = 36, boardHeight = 36;
    public GameObject [,] boardObjects = new GameObject[boardWidth, boardHeight];
    public int score, eatenPellets, totalPellets, lastGhostScore = 200;
    public float[] pelletDivision;
    public Maze mazeData;
    public Player pacMan;
    public Enemy blinky, inky, pinky, clyde;
    public bool countTime;
    public enum GameMode { Classic, PVP2P, COOP2P, GhostVPlayer }
    public GameMode currentGamemode;
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
    currentPelletsText, totalPelletsText, act1TText, act2TText, act3TText;

    [Header ("PS4 Animation")]
    public Player player;
    public Color PS4BarLightColor;
    public float currentAnimSpeed;
    public float superPelletTime;

    public GameObject createdFruit;
    public int fruitIndex;
    bool winAct;
    bool startDeath;
    bool inSuperPellet;
    int currentSirenIndex;
    float superPelletTimer;
    float timeSpent;
    Animator animator;
    DualShockGamepad ps4Gamepad;
    Transform lastPellet;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (Gamepad.all.Count > 0)
            ps4Gamepad = (DualShockGamepad)Gamepad.all[0];
        
        Debug.Log(Gamepad.all.Count);
        
        //mazeData.pelletsParent.transform.parent = null;

        for (int i = 0; i < cornerNodes.Length; i++)
            if (!cornerNodes[i].invisiblePellet)
                totalPellets++;
        
        for (int i = 0; i < allNodes.Count; i++)
            totalPellets++;

        SetGameBoard();

        totalPelletsText.text = totalPellets.ToString("D3");

        for (int i = 0; i < pelletDivision.Length; i++)
            pelletDivision[i] = (totalPellets / 6f) * (i + 1);
        
        LoadMaze();
        LoadDifficulty();
        
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

        if (currentActIndex >= mazeData.mazeActs.Length - 1)
            StartCoroutine(WinWin());
        else
            StartCoroutine(ProcessWinActAnimation());
    }

    void HandleUI()
    {
        currentScore.text = score.ToString("D6");
        currentLives.text = pacMan.pacManLives.ToString();
        currentPelletsText.text = pelletFruitCounter.ToString("D3");
        currentTime.text = TimeSpan.FromSeconds(timeSpent).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(timeSpent).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(timeSpent).Milliseconds.ToString();
    }

    void LoadMaze()
    {
        if (mazeData != null)
        {
            roundFruits = mazeData.mazeActs[currentActIndex].fruitsToAppear;
            pelletsToAppearFruit = mazeData.pelletsToAppearFruit;

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

    void LoadDifficulty()
    {
        if (difficulty != null)
        {
            difficultyName.text = difficulty.difficultyName;
            superPelletTime = difficulty.superPelletMaxDuration;
            pacMan.moveSpeed = difficulty.pacManSpeed;
            pacMan.pacManLives = difficulty.pacManStartingLives;
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
        blinky.canMove = pinky.canMove = inky.canMove = clyde.canMove = false;
        pacMan.GetComponent<Animator>().SetBool("moving", false);

        yield return new WaitForSeconds(delay);

        countTime = true;
        pacMan.canMove = true;
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

        Debug.Log(boardObjects[20, 9].name);
    }

    public void CheckForSirenChange()
    {   
        if (musicMode != MusicMode.Custom)
        {
            for (int i = 0; i < pelletDivision.Length; i++)
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
        }
    }

    public void RestartGame()
    {
        pacMan.Restart();
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

        if (musicMode != MusicMode.Custom) musicSource.clip = sirens[currentSirenIndex];
        else musicSource.clip = customMusic;

        musicSource.Play();

        startDeath = false;
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

        roundFruits = mazeData.mazeActs[currentActIndex].fruitsToAppear;
        pelletsToAppearFruit = mazeData.pelletsToAppearFruit;

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
        if (eatenPellets >= totalPellets && !winAct)
        {
            winAct = true;
            countTime = false;

            switch (currentActIndex)
            {
                case 0:
                    act1T = timeSpent;

                    act1TText.text = TimeSpan.FromSeconds(act1T).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(act1T).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(act1T).Milliseconds.ToString();
                break;

                case 1:
                    act2T = timeSpent;

                    act2TText.text = TimeSpan.FromSeconds(act2T).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(act2T).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(act2T).Milliseconds.ToString();
                break;

                case 2:
                    act3T = timeSpent;

                    act3TText.text = TimeSpan.FromSeconds(act3T).Minutes.ToString("D2") + ":" + TimeSpan.FromSeconds(act3T).Seconds.ToString("D2") + ":" +
                        TimeSpan.FromSeconds(act3T).Milliseconds.ToString();
                break;
            }

            timeSpent = 0f;

            currentActIndex++;

            if (currentActIndex >= mazeData.mazeActs.Length - 1)
                StartCoroutine(WinWin());

            StartCoroutine(ProcessWinActAnimation());
        }
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

    public void StartDeath()
    {
        if (!startDeath) startDeath = true;

        if (pacMan.pacManLives > 0)
        {
            pacMan.pacManLives--;

            if (pacMan.pacManLives <= 0)
                countTime = false;
        }

        pacMan.canMove = blinky.canMove = inky.canMove = pinky.canMove = clyde.canMove = false;
        pacMan.GetComponent<Animator>().SetBool("moving", false);
        musicSource.Stop();

        if (inSuperPellet)
        {
            superPelletTimer = 0f;
            inSuperPellet = false;
        }

        StartCoroutine(ProcessAfterDeath(2f));
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
        restartReadyText.gameObject.SetActive(true);

        if (pacMan.pacManLives <= 0)
        {
            restartReadyText.text = "GAME OVER";
            restartReadyText.color = Color.red;
        }

        yield return new WaitForSeconds(delay);

        GameOverMenu();
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

    public IEnumerator ControllerRumble(float time)
    {
        Debug.Log("here");
        ps4Gamepad.SetMotorSpeeds(0.25f, 0.25f);
        yield return new WaitForSeconds(time);
        ps4Gamepad.SetMotorSpeeds(0f, 0f);
    }

    public void ReloadGame()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    IEnumerator LoadScene(int sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            yield return null;
        }
    }

    void PS4LightBarManager()
    {
        animator.speed = currentAnimSpeed;

        if (ps4Gamepad != null)
            ps4Gamepad.SetLightBarColor(PS4BarLightColor);
    }
}
