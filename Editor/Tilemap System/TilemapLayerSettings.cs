using UnityEngine;

[CreateAssetMenu(menuName = "RPGToolkit/Map/LayerSettings")]
public class TilemapLayerSettings : ScriptableObject
{
    public string layerName;
    public TilemapLayerType layerType;
    public Color gizmoColor = Color.white;
    public int orderInLayer;
    public bool isCollidable;
    public bool isVisibleInEditor = true;
}

public enum TilemapLayerType
{
    Background,
    Obstacle,
    Foreground,
    Collision,
    Event
}