using LitMotion.Animation;
using UnityEngine;

public class PlayAnimationOnEnable : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private LitMotionAnimation litMotionAnimation;

    private void OnEnable()
    {
        litMotionAnimation.Restart();
    }

    private void Reset()
    {
        if (litMotionAnimation == null)
            litMotionAnimation = GetComponent<LitMotionAnimation>();
    }
}
