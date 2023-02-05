using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;  //摄像机焦点位置

    [SerializeField, Range(0f, 100f)]
    float Distance = 5f;    //摄像机高度

    [SerializeField, Min(0f)]
    float focusRadius = 1f; //焦点半径

    [SerializeField, Range(0f, 1f)]
    float focusCenter = 0.5f;  //焦点居中系数

    [SerializeField]
    float AlignDelay = 5f; //摄像机自动对齐延迟秒数

    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f;  //自动对齐平滑过渡的范围
    
    [SerializeField]
    Quaternion lookDirection = default; //摄像机朝向

    Vector3 focusPoint, previousFocusPoint;

    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if(focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, previousFocusPoint);
            float t = 1f;
            if(distance > 0.01f && focusCenter > 0f)
            {
                t = Mathf.Pow(1f - focusCenter, Time.unscaledDeltaTime);//用来渐变过渡
            }
            if(distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else focusPoint = targetPoint;
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    private void LateUpdate()
    {
        UpdateFocusPoint();
        Vector3 cameraDirection = lookDirection * transform.forward;
        Vector3 lookPosition = focusPoint - cameraDirection * Distance;
        transform.position = lookPosition;
    }
}
