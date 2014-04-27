using UnityEngine;
using System.Collections;

public class Kraken : MonoBehaviour {

    public string[] spawnableEnemies = new string[]
    {
        "Enemies/en_fishy",
        "Enemies/en_zombiefishy"
    };

    string currentState;

    public float detectionRange;

	// Use this for initialization
	void Start () {

        currentState = "waiting";
	
	}
	
	// Update is called once per frame
	void Update () {

        if (currentState == "waiting")
        {
            if (Vector2.Distance(GameObject.Find("Player").transform.position, transform.position) < detectionRange)
            {
                currentState = "combat";
                StartCoroutine(SpawnMobs());
            }
        }	
	}
    
    IEnumerator SpawnMobs()
    {


        while (true)
        {
            yield return new WaitForSeconds(3f);

            string mobPrefab = spawnableEnemies[Random.Range(0, spawnableEnemies.Length)];

            Vector3 enemyPosition = new Vector3(
                Random.Range(transform.position.x - 7f, transform.position.x + 7f),
                Random.Range(transform.position.y + 2f, transform.position.y + 3f),
                transform.position.z);

            GameObject spawnedMob = Instantiate(Resources.Load(mobPrefab), enemyPosition, Quaternion.identity) as GameObject;
        }

        yield return 0;
    }
}
