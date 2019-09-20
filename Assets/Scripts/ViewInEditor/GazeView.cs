using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GazeView : MonoBehaviour
{
    private Vector3[] hitPoints;
    private GameObject[] gizmos;
    // Start is called before the first frame update
    private void OnEnable()
    {
        gizmos = new GameObject[33];
        for (int i = 0; i < gizmos.Length; i++)
        {
            GameObject gizmo = new GameObject("GazeMarker");
            
            DisplayMarker mark = gizmo.AddComponent<DisplayMarker>();
            mark.Size = 0.5f;
            mark.markerType = DisplayMarker.MarkerType.Gaze;
            mark.Visible = true;
            gizmos[i] = gizmo;
        }
    }

    public void ShowGaze(Vector3[] hP)
    {
        hitPoints = hP;
                
    }
    // Uncomment to display the gaze location on the screen that the grid of rays
    // being cast to compute gaze targets. 
    private void OnDrawGizmos()
    {
        // WARNING THE COMPONENT NEEDS TO BE EXPANDED IN THE INSPECTOR !!!!!!!!!!!!!!!!
        // IF NOT THE GIZMOS WILL NOT DRAW!!!!!!!!!!!!!!
        int idx = 0;
        if (hitPoints != null)
        {
            foreach (Vector3 vec in hitPoints)
            {
                if (vec.x != Mathf.Infinity)
                {
                    gizmos[idx].transform.position = vec;
                    gizmos[idx].GetComponent<DisplayMarker>().Visible = true;
                }
                else
                {
                    gizmos[idx].GetComponent<DisplayMarker>().Visible = false;
                }
                idx += 1;
            }
        }
    }
}
