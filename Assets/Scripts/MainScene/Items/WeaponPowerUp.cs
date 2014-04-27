using UnityEngine;
using System.Collections;

public class WeaponPowerUp : MonoBehaviour {

    public string weaponPrefab;
    public Sprite displaySprite;

    private PlayerController.PlayerWeapon weaponItem;

	// Use this for initialization
	void Start () {

        LeanTween.moveY(gameObject, transform.position.y + 0.1f, 1f).setLoopPingPong().setEase(LeanTweenType.easeInOutQuad);
        
	}
	
	// Update is called once per frame
	void Update () {

        Debug.DrawLine(transform.position, GameObject.Find("Player").transform.position);

        if (Vector2.Distance(GameObject.Find("Player").transform.position, transform.position) < 1)
        {
            StartCoroutine(TriggerPowerUp());
        }
	
	}

    IEnumerator TriggerPowerUp()
    {
        LeanTween.move(gameObject, GameObject.Find("Player").transform.position, 0.1f);
        LeanTween.scale(gameObject, Vector3.zero, 0.1f);
        yield return new WaitForSeconds(0.2f);

        GameObject.Find("powerUpSoundObject").audio.Play();

        switch (weaponPrefab)
        {
            case "wp_Bubble":
                GameObject.Find("Player").GetComponent<PlayerController>().currentWeapon
                    = PlayerController.PlayerWeapon.wp_Bubble;
                break;

            case "wp_AdvBubble":
                GameObject.Find("Player").GetComponent<PlayerController>().currentWeapon
                    = PlayerController.PlayerWeapon.wp_AdvBubble;
                break;

            case "wp_Harpoon":
                GameObject.Find("Player").GetComponent<PlayerController>().currentWeapon
                    = PlayerController.PlayerWeapon.wp_Harpoon;
                break;
        }

        Destroy(gameObject);

        yield return 0;
    }
}
