using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Sprite[] animationFrames;
    public float animationFPS;

    private SpriteRenderer spriteRenderer;

    private bool isAnimating = false;
    private float referenceTime = 0f;

    private GameObject weaponOffset;

    public enum PlayerWeapon
    {
        wp_Bubble,
        wp_AdvBubble,
        wp_Harpoon
    }

    [HideInInspector]
    public PlayerWeapon currentWeapon = PlayerWeapon.wp_AdvBubble;

    public float weaponFireDelay;

    private float lastWeaponFire = 0;

    // private bool isCollidingWithEnemy = false;

    private float lastHitTime = 0f;
    public float hitInvulnerabilityTime = 1f;

    private int health = 500;

    public GUISkin hudSkin;
    public Rect healthRect;

    private bool isDead = false;

    public Texture hudHealthTexture;

    private bool gameWon = false;

	// Use this for initialization
	void Start () 
    {
        currentWeapon = PlayerWeapon.wp_Bubble;

        // ignore collisions between player and projectiles
        Physics2D.IgnoreLayerCollision(8, 9);

        weaponOffset = transform.FindChild("WeaponOffset").gameObject;

        spriteRenderer = renderer as SpriteRenderer;
	}
	
	// Update is called once per frame
	void Update () 
    {   
        Debug.DrawRay(weaponOffset.transform.position, weaponOffset.transform.right * 2f);

        if (isAnimating)
        {
            int index = (int)((Time.timeSinceLevelLoad - referenceTime) * animationFPS);
            index = index % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[index];
        }
        else
        {
            // set the sprite to the stationary one
            spriteRenderer.sprite = animationFrames[0];
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (Time.timeSinceLevelLoad >= lastWeaponFire + weaponFireDelay)
            {
                lastWeaponFire = Time.timeSinceLevelLoad;
                FireWeapon();
            }
        }
	}

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // start animating
            isAnimating = true;
            referenceTime = Time.timeSinceLevelLoad;
        }
        else if (!Input.GetKey(KeyCode.UpArrow))
        {
            // stop animating
            isAnimating = false;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            rigidbody2D.AddForce(transform.right * 10);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rigidbody2D.AddTorque(10f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rigidbody2D.AddTorque(-10f);
        }
    }

    void FireWeapon()
    {
        string weaponPrefab;

        switch (currentWeapon)
        {
            case PlayerWeapon.wp_Bubble:
                weaponPrefab = "Weapons/wp_Bubble";
                break;

            case PlayerWeapon.wp_AdvBubble:
                weaponPrefab = "Weapons/wp_AdvBubble";
                break;

            case PlayerWeapon.wp_Harpoon:
                weaponPrefab = "Weapons/wp_Harpoon";
                break;

            default:
                weaponPrefab = "Weapons/wp_Bubble";
                break;
        }

        GameObject projectile 
            = Instantiate(Resources.Load(weaponPrefab), weaponOffset.transform.position, weaponOffset.transform.rotation) as GameObject;

        weaponOffset.audio.Play();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == 11) // if object is enemy
        {
            if (Time.timeSinceLevelLoad > lastHitTime + hitInvulnerabilityTime)
            {
                int hitDamage = 0;

                switch (other.gameObject.name)
                {
                    case "en_MiniTentacle":
                        hitDamage = 10;
                        break;

                    case "en_fishy":
                        hitDamage = 20;
                        break;

                    case "en_zombiefishy":
                        hitDamage = 35;
                        break;

                    case "en_EyeEnemy":
                        hitDamage = 50;
                        break;

                    default:
                        hitDamage = 10;
                        break;
                }

                lastHitTime = Time.timeSinceLevelLoad;
                TakeDamage(hitDamage);
            }
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.layer == 11) // if object is enemy
        {
            if (Time.timeSinceLevelLoad > lastHitTime + hitInvulnerabilityTime)
            {
                int hitDamage = 0;

                switch (other.gameObject.name)
                {
                    case "en_MiniTentacle":
                        hitDamage = 10;
                        break;

                    case "en_fishy":
                        hitDamage = 20;
                        break;

                    case "en_zombiefishy":
                        hitDamage = 35;
                        break;

                    case "en_EyeEnemy":
                        hitDamage = 50;
                        break;

                    default:
                        hitDamage = 10;
                        break;
                }

                lastHitTime = Time.timeSinceLevelLoad;
                TakeDamage(hitDamage);
            }
        }
    }

    void TakeDamage(int damage)
    {
        if (!gameWon)
        {
            Debug.Log(string.Format("Player took {0} damage. Health is now {1}.", damage, health));

            if (health > damage)
            {
                health -= damage;
                audio.Play();
            }
            else
            {
                StartCoroutine(PlayerDeath());
            }
        }
    }

    IEnumerator PlayerDeath()
    {
        // Time.timeScale = 0f; // stop the game

        if (!isDead)
        {
            Camera.main.GetComponent<CameraController>().FadeScreen(false, Color.white, 0.5f);
            yield return new WaitForSeconds(1f);

            Application.LoadLevel("GameOverScene");
        }

        yield return 0;
    }

    IEnumerator WinCondition()
    {
        // game has been won

        gameWon = true;

        yield return new WaitForSeconds(3f);

        Camera.main.GetComponent<CameraController>().FadeScreen(false, Color.white, 10f);
        yield return new WaitForSeconds(13f);

        Application.LoadLevel("WinScene");
    }

    void OnGUI()
    {
        GUI.skin = hudSkin;

        GUI.DrawTexture(
            new Rect(healthRect.x - 20, healthRect.y - 20, healthRect.width + 40, healthRect.height + 40),
            hudHealthTexture);

        GUI.Label(healthRect, "HP: " + health.ToString());
    }
}
