using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {

    private LTRect tryAgainRect;
    public float buttonHeight;
    public GUIStyle buttonStyle;

	// Use this for initialization
	void Start () {

        tryAgainRect = new LTRect(0, Screen.height, Screen.width, buttonHeight);

        StartCoroutine(InitializeButton());
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator InitializeButton()
    {
        yield return new WaitForSeconds(3f);

        LeanTween.move(tryAgainRect, new Vector2(0f, Screen.height - 200f), 1f).setEase(LeanTweenType.easeInOutQuad);

        yield return 0;
    }

    void OnGUI()
    {
        if (GUI.Button(tryAgainRect.rect, "Try Again?", buttonStyle))
        {
            Application.LoadLevel("MainScene");
        }
    }
}
