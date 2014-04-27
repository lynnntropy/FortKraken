using UnityEngine;
using System.Collections;

public class MiniTentacle : MonoBehaviour {

    public bool movingUp;
    public float movementRange;

	// Use this for initialization
	void Start () {

        if (movingUp)
        {
            LeanTween.moveY(gameObject, transform.position.y + movementRange, 1f).setLoopPingPong().setEase(LeanTweenType.easeOutExpo);
        }
        else
        {
            LeanTween.moveY(gameObject, transform.position.y - movementRange, 1f).setLoopPingPong().setEase(LeanTweenType.easeOutExpo);
        }	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
