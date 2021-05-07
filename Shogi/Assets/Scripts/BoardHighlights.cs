using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class BoardHighlights : MonoBehaviour
{
    public static BoardHighlights Instance{set;get;}
    [SerializeField] private GameObject highlightPrefab;
    private List<GameObject> moveHighlights;
    private GameObject checkHighlight;
    private void Start() {
        Instance = this;
        moveHighlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(){
        // Find and return already created Highlight to not create more than necessary.
        GameObject go = moveHighlights.Find(g=> !g.activeSelf);

        if (go == null){
            go = Instantiate(highlightPrefab);
            moveHighlights.Add(go);
        }
        
        return go;
    }

    public void HighlightAllowedMoves(bool[,] moves){
        for (int x = 0; x < 9; x++){
            for (int y = 0; y < 9; y++){
                if (moves[x, y]){
                    GameObject go = GetHighlightObject();
                    go.SetActive(true);
                    go.transform.position = new Vector3(x + C.tileOffset, 0, y + C.tileOffset);
                }
            }
        }
    }
    public void HideHighlights(){
        foreach (GameObject go in moveHighlights){
            go.SetActive(false);
        }
    }
    public void HighlightCheck(int x, int y){
        if (!checkHighlight){
            checkHighlight = Instantiate(highlightPrefab);
            checkHighlight.GetComponent<Renderer>().material.color = Color.red;
        }
        checkHighlight.SetActive(true);
        checkHighlight.transform.position = new Vector3(x + C.tileOffset, 0, y + C.tileOffset);
    }
    public void HideCheckHighlight(){
        if (checkHighlight)
            if(checkHighlight.activeSelf)
                checkHighlight.SetActive(false);
    }
}
