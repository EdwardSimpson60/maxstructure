﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public abstract class PlayerTool : MonoBehaviour
{
    [HideInInspector] public bool isToggled = false;

    #region Cursor Options
    [Header("Cursor Options")]
    [Tooltip("Show a custom mouse cursor when this tool is enabled")]
    public bool useCustomCursor = false;
    
    [Tooltip("Custom cursor when the tool is enabled. Leave empty to use the system's default cursor.")]
    [DrawIf("useCustomCursor", true, DrawIfAttribute.DisablingType.DontDraw)]
    public Texture2D ToolCursorTexture;
    
    [Tooltip("Leave it as (0,0) if the default cursor is used")]
    [DrawIf("useCustomCursor", true, DrawIfAttribute.DisablingType.DontDraw)]
    public Vector2 CursorOffset = Vector2.zero;
    #endregion

    #region Input Opions
    public enum MouseClickMode
    {
        None,
        Held,
        ClickDown,
        ClickUp,
        ClickDownAndUp
    }
    
    [Header("Input Options")]
    [Tooltip("Choose when this tool is invoked using the left mouse button.\n\nOptions:\n" +
             "None: Will never invoke with the button\n" +
             "Held: Invokes while held down\n" +
             "ClickDown: Invokes when pressed down\n" +
             "ClickUp: Invokes when released\n" +
             "ClickDownAndUp: Invokes when either pressed down or released")]
    public MouseClickMode leftMouseClickMode;
    
    [Tooltip("Choose when this tool is invoked using the right mouse button.\n\nOptions:\n" +
             "None: Will never invoke with the button\n" +
             "Held: Invokes while held down\n" +
             "ClickDown: Invokes when pressed down\n" +
             "ClickUp: Invokes when released\n" +
             "ClickDownAndUp: Invokes when either pressed down or released")]
    public MouseClickMode rightMouseClickMode;

    [Header("Advanced Input Options")]
    [Tooltip("If you wish to use other input methods to invoke you tool")]
    public KeyCode[] KeyHeldInputList;
    [Tooltip("If you wish to use other input methods to invoke you tool")]
    public KeyCode[] KeyUpInputList;
    [Tooltip("If you wish to use other input methods to invoke you tool")]
    public KeyCode[] KeyDownInputList;

    
    #endregion
    
    #region UI
    [Header("User Interface")]
    [SerializeField] private ChangeColorButton ToolButton;
    public List<GameObject> UserInterfaceObjects;
    #endregion


    public virtual void Enable()
    {
        isToggled = true;
        UpdateCursor();

        if (ToolButton)
        {
            ToolButton.SetButtonState(true);
        }
    }
    
    public virtual void Disable() 
    {
        isToggled = false;
        if (ToolButton)
        {
            ToolButton.SetButtonState(false);
        }
    }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
        {
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        #endif
        
        if (UserInterfaceObjects != null)
        {
            foreach (var obj in UserInterfaceObjects)
            {
                if (obj)
                {
                    obj.SetActive(true);
                }
            }
        }

        MoveButtonToCustomToolDrawer();
    }

    private void OnDisable()
    {
        if (UserInterfaceObjects != null)
        {
            foreach (var obj in UserInterfaceObjects)
            {
                if (obj)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (UserInterfaceObjects != null)
        {
            foreach (var obj in UserInterfaceObjects)
            {
                DestroyImmediate(obj);
            }
        }
    }

    public void CheckCanUseTool()
    {
        if (!isToggled)
        {
            return;
        }

        // Pointer clicks to invoke tool
        if (Input.touchCount < 2 && !EventSystem.current.IsPointerOverGameObject(-1))
        {
            if (leftMouseClickMode == MouseClickMode.Held && Input.GetMouseButton(0) ||
                leftMouseClickMode == MouseClickMode.ClickDown && Input.GetMouseButtonDown(0) ||
                leftMouseClickMode == MouseClickMode.ClickUp && Input.GetMouseButtonUp(0) ||
                (leftMouseClickMode == MouseClickMode.ClickDownAndUp && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))))
            {
                UseTool();
            }

            if (rightMouseClickMode == MouseClickMode.Held && Input.GetMouseButton(0) ||
                rightMouseClickMode == MouseClickMode.ClickDown && Input.GetMouseButtonDown(0) ||
                rightMouseClickMode == MouseClickMode.ClickUp && Input.GetMouseButtonUp(0) ||
                (rightMouseClickMode == MouseClickMode.ClickDownAndUp && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))))
            {
                UseTool();
            }
        }
        
        // Key presses to invoke tool
        foreach (var key in KeyHeldInputList)
        {
            if (Input.GetKey(key))
            {
                UseTool();
            }
        }
        foreach (var key in KeyUpInputList)
        {
            if (Input.GetKeyUp(key))
            {
                UseTool();
            }
        }
        foreach (var key in KeyDownInputList)
        {
            if (Input.GetKeyDown(key))
            {
                UseTool();
            }
        }
    }
    
    public virtual void Toggle()
    {
        isToggled = !isToggled;

        if (isToggled)
        {
            ToolManager.instance.SwitchTools(this);
        } else
        {
            ToolManager.instance.DisableTool(this);
        }
    }

    /// <summary>
    /// Prepares the UseTool function that the child class implements
    /// </summary>
    private void UseTool()
    {
        Ray cameraRay = GameController.CurrentCamera.ScreenPointToRay(Input.mousePosition);
        // Instantiate a flag for one flag at the actual mouse position raycast
        if (!Physics.Raycast(cameraRay, out RaycastHit hit))
        {
            OnInvalidHit();
            return;
        }
        
        if (hit.transform.tag.Equals("Terrain"))
        {
            UseTool(hit);
        }
        else
        {
            OnInvalidHit();
        }
    }

    public abstract void UseTool(RaycastHit hit);

    public virtual void OnInvalidHit()
    {
        
    }

    public abstract void Undo();

    private void UpdateCursor()
    {
        if (!useCustomCursor)
        {
            return;
        }
        Cursor.SetCursor(ToolCursorTexture, CursorOffset, CursorMode.Auto);
        ToolManager.instance.activeCursor = ToolCursorTexture;
        ToolManager.instance.activeCursorOffset = CursorOffset;
    }

    private void MoveButtonToCustomToolDrawer()
    {
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            // No point in running this if we are in playmode
            return;
        }
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            // If prefab stage is not null, then we are in prefab edit mode, so we do NOT want to move the button yet
            return;
        }
        
        var button = GetComponentInChildren<CustomToolButtonTag>();
        if (button != null)
        {
            ToolButton = button.GetComponent<ChangeColorButton>();
        }
        else
        {
            return;
        }

        if (ToolButton.transform.IsChildOf(transform))
        {
            if (!UserInterfaceObjects.Contains(ToolButton.gameObject))
            {
                UserInterfaceObjects.Add(ToolButton.gameObject);
            }
            CustomToolsDrawer.Instance.AddObjectToDrawer(ToolButton.transform);
        }
        #endif
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(PlayerTool), true)]
[CanEditMultipleObjects]
public class PlayerToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        /*
        var tool = (PlayerTool) target;

        GUILayout.Space(20);
        if (GUILayout.Button("Remove Tool"))
        {
            var userInterfaceObjects = tool.UserInterfaceObjects;

            foreach (var obj in userInterfaceObjects)
            {
                Destroy(obj);
            }
        }
    */
    }
}
#endif    


public class ToolManager : MonoBehaviour
{
    private static ToolManager _instance;
    public static ToolManager instance
    {
        get
        {
            if (_instance)
            {
                return _instance;
            }

            return FindObjectOfType<ToolManager>().GetComponent<ToolManager>();
        }
        set
        {
            _instance = value;
        }
    }

    [HideInInspector] public PlayerTool activeTool;

    [HideInInspector] public UnityEvent switchToolEvent;
    [HideInInspector] public UnityEvent undoEvent;

    [HideInInspector] public Texture2D activeCursor;
    [HideInInspector] public Vector2 activeCursorOffset;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)))
        {
            // Undo
            if (activeTool != null)
            {
                //StereonetsController.singleton.Undo();
                Undo();
            }
        }
        
        if (activeTool != null)
        {
            activeTool.CheckCanUseTool();
        }
    }

    public void SwitchTools(PlayerTool tool)
    {
        if (activeTool != null)
        {
            activeTool.Disable();
        }

        tool.Enable();

        activeTool = tool;

        switchToolEvent.Invoke();
    }

    public void DisableTool(PlayerTool tool)
    {
        if (activeTool.Equals(tool))
        {
            tool.Disable();
            activeTool = null;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        activeCursor = null;
        activeCursorOffset = Vector2.zero;
    }

    public void DisableActiveTool()
    {
        if (activeTool == null)
        {
            return;
        }

        DisableTool(activeTool);
    }

    public void EnableActiveTool()
    {
        if (activeTool == null)
        {
            return;
        }
        activeTool.Enable();
    }

    public bool HasToolActive()
    {
        return activeTool != null;
    }

    public void Undo()
    {
        if (activeTool)
        {
            activeTool.Undo();
            undoEvent.Invoke();
        }
    }
}
