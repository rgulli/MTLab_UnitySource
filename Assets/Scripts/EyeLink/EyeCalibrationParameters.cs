using System.Collections.Generic;
using System;

// Structures holding the parameters from monkey logic to calibrate the eye 
// data.
[Serializable]
public class EyeCalibrationParameters
{
    public List<float> el_gains;
    public List<float> el_offsets; 
    public List<float> t_offset;
    public List<float> t_rotation;
    public List<float> t_transform;
    public float pix_per_deg;
    public int ml_x_res;
    public int ml_y_res;
    public string el_IP;
    public int el_eyeID; 

}