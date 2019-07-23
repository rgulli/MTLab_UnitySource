using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeProcessing : MonoBehaviour
{
    // We only need the pixels per degrees to convert the ~2DVA foveation windon
    // to pixels. 
    private Vector2 _gaze;
    private float _pix_per_deg = 0.0f;
    private float _foveation_radius_deg = 2.0f; // In DVA
    private float _foveation_radius_pix_x; // in pixels
    private float _foveation_radius_pix_y;
    private int _x_res;
    private int _y_res;
    private Vector3[] _rays = new Vector3[33];

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        // Get screen resolution
        cam = Camera.main;
        _x_res = cam.scaledPixelWidth;
        _y_res = cam.scaledPixelWidth;
    }

    // Response to event from MonkeyLogicController forwarding the eye calibration values from 
    // Monkeylogic. 
    public void UpdateCalibration(EyeCalibrationParameters parameters)
    {
        // Just in case there is stretching of the image, wrong aspect ration or a difference in 
        // resolution between the calibration and Unity. 
        _pix_per_deg = parameters.pix_per_deg;
        _foveation_radius_pix_x = _foveation_radius_deg * (_pix_per_deg * _x_res / parameters.ml_x_res);
        _foveation_radius_pix_y = _foveation_radius_deg * (_pix_per_deg * _y_res / parameters.ml_y_res);

        // Compute rays around the gaze position 
        // generate array of raycast transformations centered on (0,0)
        float[] angles = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        float[] radii = new float[] { 0.25f, 0.5f, .75f, 1f };

        // _rays are a series of 2D points (z=0;) centered on (0,0) along 4 concentric circles
        // there are 8 points on each circle, dividing the foveation radius along 32 points
        // to cover uniformly the middle and maximal radii are rotated by 45 degrees
        // centerpoint
        _rays[0].x = 0;
        _rays[0].y = 0;
        _rays[0].z = 0;

        int idx = 1;
        float modif = 0;
        for (int rad = 0; rad < 4; rad++)
        {
            for (int ang = 0; ang < 8; ang++)
            {
                if (rad == 1 || rad == 3)
                {
                    modif = Mathf.PI / 8;
                }
                else
                {
                    modif = 0;
                }
                _rays[idx].x = _foveation_radius_pix_x * radii[rad] * Mathf.Sin((angles[ang] / Mathf.Rad2Deg) + modif);
                _rays[idx].y = _foveation_radius_pix_y * radii[rad] * Mathf.Cos((angles[ang] / Mathf.Rad2Deg) + modif);
                _rays[idx].z = 0f;
                idx++;
            }
        }
    }

    // Called on Update from the EyeLinkController when it has the calibration value
    // and a connection to the eyelink. Input gaze position in pixels and maps to objects
    // in the environment. 
    public string ProcessGaze(Vector2 eyePix)
    {
        _gaze = eyePix;
        Dictionary<string, int> gazeDict = new Dictionary<string, int>();

        // Compute the gaze position on screen at the start and end of the view frustum
        Vector3 ptOrig = new Vector3(eyePix.x, eyePix.y, cam.nearClipPlane);
        Vector3 ptEnd = new Vector3(eyePix.x, eyePix.y, cam.farClipPlane);

        // Origin of the ray castring from the screen positions
        Vector3 worldPtOrig = new Vector3();
        Vector3 worldPtEnd = new Vector3();

        RaycastHit hit;

        // Culling mask of 0 is when the player is on black, no gaze then
        if (cam.cullingMask != 0) 
        {
            // Loop through all the rays and compute hits
            foreach (Vector3 r in _rays)
            {
                // skip if any points fall outside the screen
                if ((ptOrig.x + r.x) < 0 || (ptOrig.y + r.y) < 0 || (ptOrig.x + r.x) > _x_res || (ptOrig.y + r.y) > _y_res)
                {
                    continue;
                }

                // ScreenToWorldPoint converts the pixel position on screen 
                // (0,0) is bottom left 
                // to world position 
                // the x and y components of the vectors are in pixels on screen
                // the z component is the distance along the view frustum
                // we only care about what is rendered for ray tracing so we set the distances
                // to the near and far clipping planes. 
                worldPtOrig = cam.ScreenToWorldPoint(ptOrig + r);
                worldPtEnd = cam.ScreenToWorldPoint(ptEnd + r);

                // Raycast arguments are: 
                // Start position
                // Direction
                // IMPORTANT If you want to ignore some objects, you need to set their layer to : 2 ignore raycast. 
                Physics.Raycast(worldPtOrig, worldPtEnd - worldPtOrig, out hit, cam.farClipPlane);
                if (hit.collider != null)
                {
                    // Dictionnary contains the name of hit object and the number 
                    // of rays colliding out of the 33. 
                    if (gazeDict.ContainsKey(hit.collider.name))
                    {
                        gazeDict[hit.collider.name] += 1;
                    }
                    else
                    {
                        gazeDict.Add(hit.collider.name, 1);
                    }

                }


            }
        }

        if (gazeDict.Count == 0)
        {
            gazeDict.Add("None", 0);
        }

        return DictToString(gazeDict);
    }

    private string DictToString(Dictionary<string, int> gazeDict)
    {
        string out_string = "";

        foreach (KeyValuePair<string, int> kvp in gazeDict)
        {
            out_string += kvp.Key + "_" + kvp.Value + "/";
        }

        return out_string;
    }

    // Uncomment to display the gaze location on the screen that the grid of rays
    // being cast to compute gaze targets. 
    private void OnGUI()
    {
        Texture2D myTex = new Texture2D(1, 1);
        myTex.SetPixel(0, 0, Color.black);
        myTex.Apply();
        foreach (Vector3 r in _rays)
        {
            GUI.DrawTexture(new Rect(_gaze.x + r.x, cam.pixelHeight - _gaze.y + r.y, 2, 2), myTex);
        }
    }
}
