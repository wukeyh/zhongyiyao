using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    //存储UI Panel 的栈
    private Stack<BasePanel> UIStack;
    //存储Panel的名称和对应的物体
    private Dictionary<string, GameObject>  UIDict;
    //当前场景下对应的canvas
    private GameObject canvas;

    private static UIManager Instance;

    public static UIManager GetInstance()
    {
        if(Instance == null)
        {
            Debug.LogError("UIManager实例不存在");
            return Instance;
        }
        else return Instance;
    }

    public UIManager()
    {
        Instance = this;
    }

    public GameObject GetSingleObject(UIType uiType)
    {
        if(UIDict.ContainsKey(uiType.name))
        {
            return UIDict[uiType.name];
        }

        if(canvas == null)
        {
            Debug.LogError("UIManager未能成功获得canvas");
            return null;
        }

        GameObject gameObject = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(uiType.Path), canvas.transform);
        return gameObject;
    }

    public void Push(BasePanel basePanel)
    {
        Debug.Log($"{basePanel.GetUIType().name}被压入栈");
        
        if(UIStack.Count > 0)
        {
            UIStack.Peek().Disable();   //将栈顶元素禁用
        }

        GameObject gameObject = GetSingleObject(basePanel.GetUIType());
        UIDict.Add(basePanel.GetUIType().name, gameObject);
        basePanel.SetUIObject(gameObject);

        if(UIStack.Count == 0)
        {
            UIStack.Push(basePanel);
        }
        else
        {
            if(UIStack.Peek().GetUIType().name != basePanel.GetUIType().name)
            {
                UIStack.Push(basePanel);
            }
        }

        basePanel.Enable();
    }

    public void PopAll()
    {
        while(UIStack.Count > 0)
        {
            UIStack.Peek().Disable();
            UIStack.Peek().Destroy();
            GameObject.Destroy(UIDict[UIStack.Peek().GetUIType().name]);
            UIDict.Remove(UIStack.Peek().GetUIType().name);
            UIStack.Pop();
        }
    }

    public void PopSingle()
    {
        if(UIStack.Count > 0)
        {
            UIStack.Peek().Disable();
            UIStack.Peek().Destroy();
            GameObject.Destroy(UIDict[UIStack.Peek().GetUIType().name]);
            UIDict.Remove(UIStack.Peek().GetUIType().name);
            UIStack.Pop();

            if(UIStack.Count > 0)
            {
                UIStack.Peek().Enable();
            }
        }
    }

}
