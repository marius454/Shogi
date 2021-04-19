using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class BoardHighlights : MonoBehaviour
{
    public static BoardHighlights Instance{set;get;}
    public GameObject highlightPrefab;
    private List<GameObject> highlights;
    private void Start() {
        Instance = this;
        highlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(){
        // Find and return the first object that matches the condition
        GameObject go = highlights.Find(g=> !g.activeSelf);

        if (go == null){
            go = Instantiate(highlightPrefab);
            highlights.Add(go);
        }
        
        return go;
    }

    public void HighlightAllowedMoves(bool[,] moves){
        for (int i = 0; i < 9; i++){
            for (int j = 0; j < 9; j++){
                if (moves[i, j]){
                    GameObject go = GetHighlightObject();
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + C.tileOffset, 0, j + C.tileOffset);
                }
            }
        }
    }

    public void HideHighlights(){
        foreach (GameObject go in highlights){
            go.SetActive(false);
        }
    }
}
