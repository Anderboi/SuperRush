namespace Ryvok
{
    /// <summary>
    /// The five core inputs. Direction = where the threat is (per GDD section 6):
    /// Up = aerial, Down = ground, Left/Right = side lanes, Tap = frontal/center.
    /// </summary>
    public enum SwipeDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Tap
    }
}
