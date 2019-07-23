///<summary>
/// In UDK we had the little apple icon for the possible positions. By default in unity an empty object
/// i.e. a transform will not display anything. Since we want these objects to define possible start/target/object
/// locations, we only want the transform but we can't see them in the editor. This script adds a little colored
/// sphere on their position. 
/// 
/// Now the MarkerType is meaningless and only serves Editor display purposes (i.e. you can set a "Start" marker
/// as a target position with no problem). It's just to make things cleaner in the editor. 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMarker : MonoBehaviour
{
    public enum MarkerType
    {
        Start, 
        Target, 
        Cue, 
        Trigger,
        Null
    }
    public MarkerType markerType = MarkerType.Null;

    public bool Visible = true; 

    void OnDrawGizmos()
    {
        if (Visible)
        {
            // Draw a colored sphere at the transform's position
            switch (markerType)
            {
                case MarkerType.Start:
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(transform.position, 0.5f);
                    break;
                case MarkerType.Target:
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transform.position, 0.5f);
                    break;
                case MarkerType.Cue:
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(transform.position, 0.5f);
                    break;
                case MarkerType.Trigger:
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, transform.localScale);
                    break;
                default:
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(transform.position, 0.5f);
                    break;
            }
        }
    }

}
