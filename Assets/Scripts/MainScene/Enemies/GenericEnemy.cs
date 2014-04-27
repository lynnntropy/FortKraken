using UnityEngine;
using System.Collections;

public class GenericEnemy : MonoBehaviour {

    public int health;
    public Sprite flashingSprite;

    private Sprite originalSprite;

	// Use this for initialization
	void Start () {

        originalSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TakeDamage(int damage)
    {
        if (health > damage)
        {
            StartCoroutine(FlashAnimation());

            Debug.Log("Took " + damage + " damage!");
            Debug.LogWarning(gameObject.name + "'s health is now " + health + "!");
            health -= damage;
        }
        else
        {
            Death();
        }
    }

    public void Death()
    {
        GameObject.Find("expSoundObject").audio.Play();

        GameObject particleSystem =
            Instantiate(Resources.Load("Enemies/enemyParticleSystem"), transform.position, Quaternion.identity) as GameObject;

        if (gameObject.name == "en_Kraken")
        {
            // trigger win condition if the kraken is defeated
            GameObject.Find("Player").GetComponent<PlayerController>().StartCoroutine("WinCondition");
        }

        Destroy(gameObject);
    }

    IEnumerator FlashAnimation()
    {
        Debug.Log(gameObject.name + " is flashing!");

        if (gameObject.GetComponent<SpriteRenderer>().sprite != flashingSprite)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = flashingSprite;

            yield return new WaitForSeconds(0.15f);

            gameObject.GetComponent<SpriteRenderer>().sprite = originalSprite;
        }

        yield return 0;
    }
}
