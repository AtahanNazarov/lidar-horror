using UnityEngine;

public class HeadHider : MonoBehaviour
{
    public Transform headBone;

    void LateUpdate()
    {
        if (headBone != null)
        {
            headBone.localScale = Vector3.zero;
        }
    }
}