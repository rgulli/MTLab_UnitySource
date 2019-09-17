///<summary>
///     We will flash the screen every 3-10 frames randomly to give time to the 
///     photodiode to plateau instead of flashing every frame. 
///     
///     Canvas objects are rendered after the scene so even if we change it during
///     update, the screen value will only change at the end of the frame. 
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class PhotoDiodeFlash : MonoBehaviour
{
    // Framerate control
    //public float Rate = 10.0f;
    //float currentFrameTime;

    private int nFrames;
    private int countFrames = -1;
    private float greyScale;
    private Image square;

    private void OnGUI()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        square = gameObject.GetComponentInChildren<Image>();


        /*FullScreenView gw = ScriptableObject.CreateInstance<FullScreenView>();
        gw.autoRepaintOnSceneChange = true;
        
        gw.ShowModalUtility();
        gw.minSize = new Vector2 { x = 640, y = 480 };
        gw.position = new Rect { x = 0, y = 0, width = 640, height = 480 };     */
        
        
        var windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
        foreach (var window in windows)
        {
            
            if (window != null && window.GetType().FullName == "UnityEditor.GameView")
            {
                var wd = (EditorWindow)ScriptableObject.CreateInstance(window.GetType().FullName);

                wd.ShowUtility();
                
                window.minSize = new Vector2 { x = 1920, y = 1150 };
                wd.position = new Rect { x = 0, y = 0, width = 1920, height = 1150 };
            }
        }
        
        /*
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        currentFrameTime = Time.realtimeSinceStartup;
        StartCoroutine("WaitForNextFrame");
        */
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(1/Time.deltaTime);
        if (countFrames == -1)
        {
            countFrames = 0;
            nFrames = Random.Range(50, 100);
            greyScale = Random.Range(0.0f, 1.0f);
        }
        else if (countFrames == nFrames)
        {
            // Reset counter, next frame will define range
            countFrames = -1;
        }
        else if (countFrames < nFrames)
        {
            countFrames += 1;
        }

        if (square != null)
        {
            Color rgb = new Color() { r = greyScale, g = greyScale, b = greyScale, a = 1 };
            square.color = rgb;

            // Send data to the experiment controller to be saved on the frame stream
            EventsController.instance.SendPhotoDiodeUpdate(greyScale);

        }
    }
    /*
    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1.0f / Rate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.005f;
            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 100));
            while (t < currentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }
    */
}
