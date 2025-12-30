using UnityEngine;

public class Constants
{
    public const float DragSpeed = 75f;
    public const float MaxDragSpeed = 100000f;
    public static Vector3 CardDragOffset = new(0.1f, 0.1f, 0);

    public const int DefaultSortingOrder = 1;
    public const int DragSortingOrder = 100;
    
    public const float MinCollisionDistance = 0.5f;
    
    
    public const float MaxSliderValue = 0.675f;
    public const float TimeAnimationClient = 0.5f;
    public const float TimeServeClient = 2f;
}