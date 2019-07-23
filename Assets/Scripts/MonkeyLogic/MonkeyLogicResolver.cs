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

    public List<LSLStreamInfoWrapper> knownStreams = new List<LSLStreamInfoWrapper>(); // modified by GD to instantiate the list variable. 
    public float forgetStreamAfter = 1.0f;
    public delegate void StreamFound(LSLStreamInfoWrapper wrapper);
    public StreamFound OnStreamFound;
    public delegate void StreamLost(LSLStreamInfoWrapper wrapper);
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

    public bool IsStreamAvailable(out LSLStreamInfoWrapper info, string streamName = "", string streamType = "", string hostName = "")
    {
        var result = knownStreams.Where(i =>

        (streamName == "" || i.Name.Equals(streamName)) &&
        (streamType == "" || i.Type.Equals(streamType)) &&
        (hostName == "" || i.Type.Equals(hostName))
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
                if (!results.Any(r => r.name().Equals(item.Name)))
                {
                    OnStreamLost?.Invoke(item);
                }
            }

            // remove lost streams from cache
            knownStreams.RemoveAll(s => !results.Any(r => r.name().Equals(s.Name)));

            // add new found streams to the cache
            foreach (var item in results)
            {
                if (!knownStreams.Any(s => s.Name == item.name() && s.Type == item.type()))
                {
                    var newStreamInfo = new LSLStreamInfoWrapper(item);
                    knownStreams.Add(newStreamInfo);
                    OnStreamFound?.Invoke(newStreamInfo);
                }
            }

            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return null;
    }
}
