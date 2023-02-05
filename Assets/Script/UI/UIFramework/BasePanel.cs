using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel
{
    private UIType UI;
    private GameObject UIobj;         //挂载UI的物体

    public BasePanel(UIType ui)
    {
        this.UI = ui;
    } 

    public UIType GetUIType() =>  UI;
    public void SetUIType(UIType uI) => UI = uI;

    public GameObject GetUIObject() => UIobj;
    public void SetUIObject(GameObject uiObject) => UIobj = uiObject;


    public virtual void Start() {}

    public virtual void Enable() {}

    public virtual void Disable() {}

    public virtual void Destroy() {}
}
