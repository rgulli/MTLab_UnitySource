using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class MonkeyLogicOutlet : MonoBehaviour
{
    // Outlets settings
    public string StreamName;
    public string StreamType;
    public string UniqueID;
    public int ChannelCount;

    private List<liblsl.StreamOutlet> _outlets = new List<liblsl.StreamOutlet>();
    private List<liblsl.StreamInfo> _lslStreamInfos = new List<liblsl.StreamInfo>();
    private List<string> _outletsName = new List<string>(); 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public int Configure(string name, string type, int chan_count, double rate, liblsl.channel_format_t format, string unique_id)
    {
        StreamName = name;
        StreamType = type;
        UniqueID = unique_id;
        ChannelCount = chan_count;
        int idx;

        if (_outletsName.IndexOf(name) == -1)
        {
            _lslStreamInfos.Add( new liblsl.StreamInfo(
                                        name,
                                        type,
                                        chan_count,
                                        rate,
                                        format,
                                        unique_id));

            _outlets.Add(new liblsl.StreamOutlet(_lslStreamInfos[_lslStreamInfos.Count-1]));
            _outletsName.Add(name);
            idx = _outletsName.Count - 1;
        }
        else
        {
            idx = _outletsName.IndexOf(name);
        }
         return idx;
    }

    // Write now. 
    public void Write(int outletID, string marker)
    {
        string[] sample = { marker };
        if (outletID < _outlets.Count)
        {
            if (_outlets[outletID] != null)
                _outlets[outletID].push_sample(sample);
        }
    }
    public void Write(int outletID, double[] sample)
    {
        if (outletID < _outlets.Count)
        {
            if (_outlets[outletID] != null)
                _outlets[outletID].push_sample(sample);
        }
    }
}
