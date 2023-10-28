using UnityEngine;

public class DisableInBuild : MonoBehaviour
{
#if !UNITY_EDITOR
    // This code will be active in the build
    void Start()
    {
        // Disable the GameObject during runtime in the build
        gameObject.SetActive(false);
    }
#endif
}