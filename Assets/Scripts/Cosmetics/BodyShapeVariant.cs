using UnityEngine;

[System.Serializable]
public class BodyShapeVariant
{
    public Sprite regular;
    public Sprite athletic;
    public Sprite muscly;
    public Sprite curvy;
    public Sprite chunky;
    public Sprite slinky;

    public Sprite Get(int bodyShapeId)
    {
        return bodyShapeId switch
        {
            1 => athletic,
            2 => muscly,
            3 => curvy,
            4 => chunky,
            5 => regular,
            6 => slinky,
        };
    }
}