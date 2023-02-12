using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// [System.Serializable]
// public class EventsVector3 : UnityEvent<Vector3> { }
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;
    public Texture2D point, doorway, attack, target, arrow; 
    RaycastHit hitInfo;
    
    public event Action<Vector3> OnMouseClicked; // 一个事件获取目标坐标
    public event Action<GameObject> OnEnemyClicked;


    void Awake(){
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
    }

    void Update(){
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 返回射线

        if (Physics.Raycast(ray, out hitInfo))
        {
            // 切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto); // 图片，偏移量，模式自动不变
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto); // 图片，偏移量，模式自动不变
                    break;
            }
        }
    }

    void MouseControl(){
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point); // 触发启用时所有注册方法都会被调用
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject); 
        }
    }
}
