using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class BoardHighlights : MonoBehaviour
{
    public static BoardHighlights Instance { set; get; }
    [SerializeField] private GameObject highlightPrefab;
    private List<GameObject> allHighlights;
    private List<GameObject> moveHighlights;
    private GameObject checkHighlight;
    private GameObject lastMoveHighlight;
    private GameObject selectionHighlight;
    private void Start() {
        Instance = this;
        moveHighlights = new List<GameObject>();
        allHighlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(){
        // Find and return already created Highlight to not create more than necessary.
        GameObject go = moveHighlights.Find(g=> !g.activeSelf);

        if (!go){
            go = Instantiate(highlightPrefab);
            moveHighlights.Add(go);
            allHighlights.Add(go);
        }
        
        return go;
    }

    public void HighlightAllowedMoves(bool[,] moves){
        HideMoveHighlights();
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
    public void HideMoveHighlights(){
        foreach (GameObject go in moveHighlights){
            go.SetActive(false);
        }
    }
    public void HighlightCheck(int x, int y){
        if (!checkHighlight){
            checkHighlight = Instantiate(highlightPrefab);
            checkHighlight.GetComponent<Renderer>().material.color = Color.red;
            allHighlights.Add(checkHighlight);
        }
        checkHighlight.SetActive(true);
        checkHighlight.transform.position = new Vector3(x + C.tileOffset, 0, y + C.tileOffset);
    }
    public void HideCheckHighlight(){
        if (checkHighlight)
            if(checkHighlight.activeSelf)
                checkHighlight.SetActive(false);
    }

    public void HighlightLastMove(int x, int y){
        HideLastMoveHighlight();
        if (!lastMoveHighlight){
            lastMoveHighlight = Instantiate(highlightPrefab);
            lastMoveHighlight.GetComponent<Renderer>().material.color = new Color(0f, 0.6f, 0f, 1f);
            allHighlights.Add(lastMoveHighlight);
        }
        lastMoveHighlight.SetActive(true);
        lastMoveHighlight.transform.position = new Vector3(x + C.tileOffset, 0, y + C.tileOffset);
    }
    public void HideLastMoveHighlight(){
        if (lastMoveHighlight)
            if(lastMoveHighlight.activeSelf)
                lastMoveHighlight.SetActive(false);
    }

    public void HighlightSelection(int x, int y){
        if (!selectionHighlight){
            selectionHighlight = Instantiate(highlightPrefab);
            selectionHighlight.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            allHighlights.Add(selectionHighlight);
        }
        selectionHighlight.SetActive(true);
        selectionHighlight.transform.position = new Vector3(x + C.tileOffset, 0, y + C.tileOffset);
    }
    public void HideSelectionHighlight(){
        if (selectionHighlight)
            if(selectionHighlight.activeSelf)
                selectionHighlight.SetActive(false);
    }
    public void HideAllHighlights(){
        foreach (GameObject go in allHighlights){
            go.SetActive(false);
        }
    }
}
