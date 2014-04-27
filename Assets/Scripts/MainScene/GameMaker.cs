using UnityEngine;
using System.Collections;

public class GameMaker : MonoBehaviour {

    public string backgroundSpritePrefab;
    public int bgTileX;
    public int bgTileY;
    public Vector2 bgPivot;

	// Use this for initialization
	void Start () 
    {
        Physics2D.IgnoreLayerCollision(9, 9);

        TileBackground(bgTileX, bgTileY, backgroundSpritePrefab, bgPivot);

        StartCoroutine(StartGame());
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    IEnumerator StartGame()
    {
        Camera.main.GetComponent<CameraController>().FadeScreen(true, Color.black, 5f);
        yield return new WaitForSeconds(5f);
    }

    void TileBackground(int x, int y, string bgSprite, Vector2 pivot)
    {
        GameObject bgParent = new GameObject("TiledBackground");

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                GameObject objectInstance = Instantiate(Resources.Load(bgSprite), Vector3.zero, Quaternion.identity) as GameObject;

                Vector2 spritePosition = new Vector2(
                    pivot.x + j * objectInstance.GetComponent<SpriteRenderer>().bounds.size.x,
                    pivot.y + i * objectInstance.GetComponent<SpriteRenderer>().bounds.size.y);

                GameObject currentTile = Instantiate(Resources.Load(bgSprite), spritePosition, Quaternion.identity) as GameObject;
                currentTile.transform.parent = bgParent.transform;

                Destroy(objectInstance);
            }
        }
    }
}
