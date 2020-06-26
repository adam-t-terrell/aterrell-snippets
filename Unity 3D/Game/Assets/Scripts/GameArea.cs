using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines Rectangular area for the game
/// </summary>
[AddComponentMenu("GameArea Menu")]
public class GameArea : MonoBehaviour
{
    private Rect area;
    public Rect Area
    {
        get
        {
            return area;
        }
        set
        {
            area = value;
        }
    }

    public Vector2 size;
    public Color GizmoOutlineColor = new Color(1, 1, 0);
    public Color GizmoAreaColor = new Color(1, 1, 0, 0.2f);

    public void SetArea(Vector2 size)
    {
        float x = size.x * 0.5f;
        float y = size.y * 0.5f;
        Area = new Rect(-x,-y,x,y);
    }

    private void Awake()
    {
        SetArea(size);
    }

    private void OnValidate()
    {
        SetArea(size);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = GizmoAreaColor;
        Gizmos.DrawCube(Vector3.zero, new Vector3(Area.width, Area.height, 0));
        Gizmos.color = GizmoOutlineColor;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(Area.width, Area.height, 0));
    }
}
