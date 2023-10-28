using UnityEngine;
using UnityEngine.UI;

public class FaceShaderController : MonoBehaviour
{
    #region variables
    public Material material; // Face Tattoo Material

    public GameObject LoadingScreen;
    public TextToImage TTI;
    public InputField promptInput;

    float initialTouchDistance;
    float initialScale;
    #endregion

    #region Tattoo position and scale
    void Update()
    { 
        //position
        if (Input.touchCount == 1)//only if exacelly 1 finger is down to not get scaling and moving at the same time
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.tag == "Face")
                {
                    // Get UV coordinates
                    Vector2 uv = hit.textureCoord;
                    material.SetFloat("_XPos", uv.x);
                    material.SetFloat("_YPos", uv.y);
                }
            }
        }

        //scale
        if (Input.touchCount >= 2)// Check for multi-touch input
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                // Calculate the initial distance between touches
                initialTouchDistance = Vector2.Distance(touch0.position, touch1.position);
                initialScale = Mathf.Clamp(material.GetFloat("_Scale"), 0.1f, 10f);
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);

                // Calculate the zoom factor as the ratio of the current distance to the initial distance
                float zoomFactor = currentTouchDistance / initialTouchDistance;

                float totalZoom = Mathf.Clamp(initialScale - (1 - zoomFactor), 0.1f, 10f);

                material.SetFloat("_Scale", totalZoom);
            }
        }

    }
    #endregion

    #region Text To Image tattoo generation
    private void OnEnable() //when enabled, add SetTattooTexture() to TexttoImage handler, to prevent user error of manually adding
    {
        TTI.OnImageOutputUpdate.AddListener(SetTattooTexture);
    }
    private void OnDisable() //When disabled, remove SetTattooTexture() for clean code
    {
        TTI.OnImageOutputUpdate.RemoveListener(SetTattooTexture);
    }
    public void GenerateImage() //assigned to UI button
    {
        LoadingScreen.SetActive(true);
        TTI.PostTextToImageRequest(promptInput.text == "" ? "dog" : promptInput.text); //if it is null, get the tattoo of a dog
    }

    public void SetTattooTexture(Texture2D texture) //assigned to TTI
    {
        LoadingScreen.SetActive(false);
        material.SetTexture("_Tattoo", texture);
    }
    #endregion
}
