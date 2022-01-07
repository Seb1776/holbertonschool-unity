using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public int health = 5;
    public float speed;
    public Text scoreText, healthText, winText;
    public Image winLoseBG;

    int score = 0;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        rb.MovePosition(transform.position + playerInput * speed * Time.deltaTime);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("menu");
    }

    void SetScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    void SetHealthText()
    {
        healthText.text = "Health: " + health.ToString();
    }

    void WinGame()
    {
        winLoseBG.gameObject.SetActive(true);
        winText.color = Color.black;
        winText.text = "You Win!";
        winLoseBG.color = Color.green;
        StartCoroutine(LoadScene(3f));
    }

    void LoseGame()
    {
        winLoseBG.gameObject.SetActive(true);
        winText.color = Color.white;
        winText.text = "You Lose!";
        winLoseBG.color = Color.red;
        StartCoroutine(LoadScene(3f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            score++;
            SetScoreText();
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Trap"))
        {   
            if (health > 0)
                health--;
            
            if (health <= 0)
                LoseGame();
            
            SetHealthText();
        }

        if (other.CompareTag("Goal"))
            WinGame();
    }

    IEnumerator LoadScene(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
