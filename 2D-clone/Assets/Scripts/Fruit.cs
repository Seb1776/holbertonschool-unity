using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public int scoreValue;
    public float lifeTime;
    public AudioClip collectedFruit;

    GameManager manager;

    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void CollectedFruit()
    {
        manager.fruits[manager.fruitIndex].GetComponent<Animator>().ResetTrigger("lock");
        manager.fruits[manager.fruitIndex].GetComponent<Animator>().SetTrigger("collected");
        manager.fruitIndex++;
        manager.pelletFruitCounter = 0;

        if (manager.fruitIndex >= manager.fruits.Length - 1)
            manager.ateAllFruits = true;

        manager.createdFruit = null;
        manager.score += scoreValue;
        Destroy(this.gameObject);
    }

    public void DestroyFruit()
    {
        StartCoroutine(WaitToDestroy());
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(lifeTime);
        manager.fruitIndex++;
        manager.pelletFruitCounter = 0;
        Destroy(this.gameObject);
    }

    public void InstaDestroyFruit()
    {
        Destroy(this.gameObject);
        manager.fruitIndex++;
        manager.pelletFruitCounter = 0;
    }
}
