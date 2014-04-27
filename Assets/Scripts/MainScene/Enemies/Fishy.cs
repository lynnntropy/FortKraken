using UnityEngine;
using System.Collections;

public class Fishy : MonoBehaviour {

    public float followRange;

    private GameObject playerObject;

	// Use this for initialization
	void Start () {

        playerObject = GameObject.Find("Player");
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Vector2.Distance(playerObject.transform.position, transform.position) <= followRange)
        {
            // look at player

            Vector3 diff = playerObject.transform.position - transform.position;
            diff.Normalize();

            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 180);

            // swim towards player

            rigidbody2D.AddForce(-transform.right * 7f);
        }
	
	}
}
