using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEditor.Modules;
using System.Globalization;
using UnityEngine.Rendering;
using System.Linq;
using JetBrains.Annotations;
public class FullScreenView : EditorWindow
{
    string myString = "Hello World";
    public RenderTexture m_renderTexture;

    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    
    
    void OnGUI()
    {
        EventType type = Event.current.type;
        m_renderTexture = RenderTexture.active;
        
        if (type == EventType.Repaint)
        {
            Graphics.DrawTexture(new Rect { x = 0, y = 0, width = 1920, height = 1080 }, m_renderTexture);
        }
            
        //Handles.DrawCamera(new Rect { x = -1920/2, y = 1080/2, width = 1920, height = 1080 }, Camera.main);
        /*
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Text Field", myString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();*/
    }

    
}