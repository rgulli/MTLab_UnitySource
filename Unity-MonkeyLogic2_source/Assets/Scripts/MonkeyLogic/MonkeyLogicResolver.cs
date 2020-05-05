///<summary>
/// Copy-Pasted the base Resolver class to correct for a few bugs in the code. 
/// 
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LSL;
using UnityEngine.EventSystems;
using Assets.LSL4Unity.Scripts;
/// <summary>
/// Encapsulates the lookup logic for LSL streams with an event based appraoch
/// your custom stream inlet implementations could be subscribed to the On
/// </summary>
public class MonkeyLogicResolver : MonoBehaviour, IEventSystemHandler
{

    public List<liblsl.StreamInfo> knownStreams = new List<liblsl.StreamInfo>(); // modified by GD to instantiate the list variable. 
    public float forgetStreamAfter = 1.0f;
    public delegate void StreamFound(liblsl.StreamInfo wrapper);
    public StreamFound OnStreamFound;
    public delegate void StreamLost(liblsl.StreamInfo wrapper);
    public StreamLost OnStreamLost;

    private liblsl.ContinuousResolver resolver;
    private bool resolve = true;

    // Use this for initialization
    void Start()
    {

    }

   // Modified by GD, put the contents of the Start function here. Since we script the addition of the components, 
   // we need to wait for the listeners to be properly configured before starting the stream resolution.
   public void Run()
    {
        resolver = new liblsl.ContinuousResolver(forgetStreamAfter);

        StartCoroutine(resolveContinuously());
    }

    public void Stop()
    {
        resolve = false;
    }

    public bool IsStreamAvailable(out liblsl.StreamInfo info, string streamName = "", string streamType = "", string hostName = "")
    {
        var result = knownStreams.Where(i =>

        (streamName == "" || i.name().Equals(streamName)) &&
        (streamType == "" || i.type().Equals(streamType)) &&
        (hostName == "" || i.source_id().Equals(hostName))
        );

        if (result.Any())
        {
            info = result.First();
            return true;
        }
        else
        {
            info = null;
            return false;
        }
    }

    private IEnumerator resolveContinuously()
    {
        while (resolve)
        {
            var results = resolver.results();

            foreach (var item in knownStreams)
            {
                if (!results.Any(r => r.name().Equals(item.name())))
                {
                    OnStreamLost?.Invoke(item);
                }
            }

            // remove lost streams from cache
            knownStreams.RemoveAll(s => !results.Any(r => r.name().Equals(s.name())));

            // add new found streams to the cache
            foreach (var item in results)
            {
                if (!knownStreams.Any(s => s.name() == item.name() && s.type() == item.type()))
                {
                    knownStreams.Add(item);
                    OnStreamFound?.Invoke(item);
                }
            }

            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return null;
    }
}
