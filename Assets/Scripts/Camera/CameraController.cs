using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Texture whiteTexture;
    private Color fadeTextureColor;

    public bool trackPlayer;
    public GameObject playerObject;
    public Vector2 followBounds;    

	// Use this for initialization
	void Start () 
    {        
        fadeTextureColor = new Color(
            Color.white.r,
            Color.white.g,
            Color.white.b,
            0f);	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (trackPlayer)
        {
            if (playerObject.transform.position.x >= transform.position.x + followBounds.x)
            {
                transform.position = new Vector3(
                    playerObject.transform.position.x - followBounds.x,
                    transform.position.y,
                    transform.position.z);
            }
            else if (playerObject.transform.position.x <= transform.position.x - followBounds.x)
            {
                transform.position = new Vector3(
                    playerObject.transform.position.x + followBounds.x,
                    transform.position.y,
                    transform.position.z);
            }

            if (playerObject.transform.position.y >= transform.position.y + followBounds.y)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    playerObject.transform.position.y - followBounds.y,
                    transform.position.z);                
            }
            else if (playerObject.transform.position.y <= transform.position.y - followBounds.y)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    playerObject.transform.position.y + followBounds.y,
                    transform.position.z);                
            }

            if (Vector2.Distance(GameObject.Find("en_Kraken").transform.position, transform.position) < 10)
            {
                trackPlayer = false;

                transform.position = new Vector3(
                    GameObject.Find("en_Kraken").transform.position.x,
                    GameObject.Find("en_Kraken").transform.position.y,
                    transform.position.z);
            }
        }	
	}

    public void FadeScreen (bool fadeIn, Color fadeColor, float fadeTime)
    {
        fadeTextureColor = fadeColor;

        if (fadeIn)
        {
            LeanTween.value(gameObject, SetFadeAlpha, 1.0f, 0.0f, fadeTime);
        }
        else
        {
            LeanTween.value(gameObject, SetFadeAlpha, 0.0f, 1.0f, fadeTime);
        }
    }

    void OnGUI()
    {
        GUI.color = fadeTextureColor;

        GUI.DrawTexture(
            new Rect(0, 0, Screen.width, Screen.height),
            whiteTexture);
    }

    void SetFadeAlpha(float alpha)
    {
        fadeTextureColor = new Color(
            fadeTextureColor.r,
            fadeTextureColor.g,
            fadeTextureColor.b,
            alpha);
    }
}
