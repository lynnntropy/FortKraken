using UnityEngine;
using System.Collections;

public class IntroScreen : MonoBehaviour {

    public GameObject boatObject;
    public GameObject waterObject;
    public GameObject playerObject;
    public GameObject sunObject;

    public float boatMovementRange;
    public float boatMovementTime;

    public float waterMovementTime;

    private string captionString;
    private bool captionEnabled;

    public float captionOffset;
    public GUIStyle captionStyle;

    private bool boatTweening = true;


	// Use this for initialization
	void Start () 
    {
        sunObject.transform.position = new Vector3(
            Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - sunObject.GetComponent<SpriteRenderer>().bounds.size.x / 2,
            sunObject.transform.position.y,
            sunObject.transform.position.z);

        StartCoroutine(TweenBoat());
        StartCoroutine(TweenWater());

        StartCoroutine(StoryRoutine());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator StoryRoutine()
    {
        Camera.main.GetComponent<CameraController>().FadeScreen(true, Color.black, 3f);

        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(DisplayCaption("Up until that day, nobody suspected a thing.", 5f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DisplayCaption("The seabed was only explored by madmen..", 5f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DisplayCaption("..Who never returned from their voyage.", 5f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DisplayCaption("On that day, everything changed..", 5f));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DisplayCaption("..When THEY attacked.", 5f));
        // yield return new WaitForSeconds(7f);

        StopCoroutine("TweenBoat");
        LeanTween.cancel(boatObject);
        boatTweening = false;

        // TentacleTake

        // Track1 (Translation)
        GameObject track2OBJ = GameObject.Find("spr_Tentacle");
        track2OBJ.transform.position = new Vector3(3.967498f, -3.840069f, 0f); // Set Initial Position
        AMTween.MoveTo(track2OBJ, AMTween.Hash("delay", 0f, "time", 2.041667f, "position", new Vector3(3.162237f, -2.531516f, 0f), "easetype", "easeInOutQuad"));
        AMTween.MoveTo(track2OBJ, AMTween.Hash("delay", 2.041667f, "time", 2.083333f, "position", new Vector3(-2.45f, 1.24f, 0f), "easetype", "easeInOutQuad"));
        AMTween.MoveTo(track2OBJ, AMTween.Hash("delay", 4.125f, "time", 0.4166667f, "position", new Vector3(-4.714787f, -0.3956867f, 0f), "easetype", "easeInOutQuad"));
        AMTween.MoveTo(track2OBJ, AMTween.Hash("delay", 4.541667f, "time", 0.2083333f, "position", new Vector3(-4.714782f, -5.977065f, 0f), "easetype", "linear"));

        // Track2 (Rotation)
        GameObject track3OBJ = track2OBJ; // or use GameObject.Find("spr_Tentacle");
        track3OBJ.transform.rotation = new Quaternion(0f, 0f, 0f, 1f); // Set Initial Rotation
        AMTween.RotateTo(track3OBJ, AMTween.Hash("delay", 4.125f, "time", 0.4166667f, "rotation", new Vector3(0f, 0f, 40.79998f), "easetype", "linear"));

        // Track3 (Translation)
        GameObject track4OBJ = GameObject.Find("spr_Boat");
        track4OBJ.transform.position = new Vector3(-5.402687f, -0.8655984f, 0f); // Set Initial Position
        AMTween.MoveTo(track4OBJ, AMTween.Hash("delay", 4.625f, "time", 0.125f, "position", new Vector3(-5.402687f, -3.73f, 0f), "easetype", "linear"));

        yield return new WaitForSeconds(6f);

        Camera.main.GetComponent<CameraController>().FadeScreen(false, Color.black, 6f);
        yield return new WaitForSeconds(8f);

        Application.LoadLevel("MainScene");

        yield return 0;
    }

    IEnumerator TweenBoat()
    {
        while(boatTweening)
        {
            LeanTween.moveY(boatObject, boatObject.transform.position.y + boatMovementRange, boatMovementTime);
            yield return new WaitForSeconds(boatMovementTime);

            LeanTween.moveY(boatObject, boatObject.transform.position.y - boatMovementRange, boatMovementTime);
            yield return new WaitForSeconds(boatMovementTime);
        }
    }

    IEnumerator TweenWater()
    {
        Vector3 waterObjectOriginalPos = waterObject.transform.position;

        while (true)
        {   
            GameObject waterClone = Instantiate(
                waterObject,
                new Vector3(waterObject.transform.position.x - waterObject.GetComponent<SpriteRenderer>().bounds.size.x, waterObject.transform.position.y, waterObject.transform.position.z),
                Quaternion.identity) as GameObject;

            LeanTween.moveX(waterClone, waterClone.transform.position.x + waterObject.GetComponent<SpriteRenderer>().bounds.size.x, waterMovementTime);
            LeanTween.moveX(waterObject, waterObject.transform.position.x + waterObject.GetComponent<SpriteRenderer>().bounds.size.x, waterMovementTime);
            yield return new WaitForSeconds(waterMovementTime);

            waterObject.transform.position = waterObjectOriginalPos;
            Destroy(waterClone);
        }
    }

    IEnumerator DisplayCaption(string line, float time)
    {
        captionString = line;
        captionEnabled = true;

        yield return new WaitForSeconds(time);

        captionEnabled = false;

        yield return 0;
    }

    void OnGUI()
    {
        if (captionEnabled)
        {
            Rect captionRect = new Rect(
                0, Screen.height - captionOffset, Screen.width, 500);

            GUI.Label(captionRect, captionString, captionStyle);
        }
    }
}
