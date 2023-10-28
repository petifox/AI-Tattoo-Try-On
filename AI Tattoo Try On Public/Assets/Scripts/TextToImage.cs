using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Events;

public class TextToImage : MonoBehaviour
{
    #region variables
    public int Resolution = 512;
    public int Steps = 20; //Itheration count
    public int Cfg_scale = 10; //CFG scale adjusts how similat the image looks to the prompt
    public string Style_preset = "line-art";
    public string BadPrompt = "complex, blurry, bad, grey, vignette, gradient";
    public string GoodPrompt = "clean, simple, black and white tattoo, pure white background, tattoo of a ";

    [HideInInspector] public string UserPrompt; //for example "Dog"

    public UnityEvent<Texture2D> OnImageOutputUpdate; //for when the AI generated resoults come back

    private const string apiURL = "https://api.stability.ai/v1/generation/stable-diffusion-512-v2-1/text-to-image";
    private const string apiKey = "Your API Key";
    #endregion

    /// <summary>
    /// This function uses the UserPrompt to get an image from stability.ai <a href="https://platform.stability.ai/docs/api-reference#tag/v1generation/operation/textToImage"> and broadcasts the resoults in <see cref="OnImageOutputUpdate"/>
    /// </summary>
    /// <param name="UserPrompt">the prompt given by the user for example "dog"</param>
    /// <see cref=""/>
    public void PostTextToImageRequest(string UserPrompt) { StartCoroutine(PostTextToImageRequestEnumerator(UserPrompt)); } //more elegant way to call IEnumerator

    IEnumerator PostTextToImageRequestEnumerator(string UserPrompt)
    {
        RequestBody body = new RequestBody
        {
            steps = Steps,
            width = Resolution,
            height = Resolution,
            seed = 0,
            cfg_scale = Cfg_scale,
            samples = 1,
            style_preset = Style_preset,
            text_prompts = new List<TextPrompt>
            {
                new TextPrompt { text = GoodPrompt + UserPrompt, weight = 1 }, //add the constant GoodPrompt and the changing UserPrompt together
                new TextPrompt { text = BadPrompt, weight = -1 } //what the AI should avoid
            }
        };

        string bodyJson = JsonUtility.ToJson(body); //to Json
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson); //to bytes

        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" },
            { "Authorization", "Bearer " + apiKey }
        };

        using (UnityWebRequest www = new UnityWebRequest(apiURL, "POST")) //make new api POST request
        {
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            foreach (var header in headers)
            {
                www.SetRequestHeader(header.Key, header.Value);
                OnImageOutputUpdate.Invoke(new Texture2D(2, 2));
            }

            yield return www.SendWebRequest(); //send and wait for resoults

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) //handle errors
            {
                Debug.LogError("Error: " + www.error);
            }
            else //handle resoults
            {
                byte[] results = www.downloadHandler.data; //read resoults
                string json = System.Text.Encoding.UTF8.GetString(results); //to Json
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(json); //structure

                byte[] imageBytes = Convert.FromBase64String(responseData.artifacts[0].base64); //get the image stored in text Base64 format to bytes
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes); //finished textures

                OnImageOutputUpdate.Invoke(texture); //call the unityEvent to broadcast the new tattoo
            }
        }
    }

    #region helper classes
    [Serializable]
    public class TextPrompt
    {
        public string text;
        public int weight;
    }

    [Serializable]
    public class RequestBody
    {
        public int steps;
        public int width;
        public int height;
        public int seed;
        public int cfg_scale;
        public int samples;
        public string style_preset;
        public List<TextPrompt> text_prompts;
    }

    [Serializable]
    private class Artifact
    {
        public int seed;
        public string base64;
    }

    [Serializable]
    private class ResponseData
    {
        public Artifact[] artifacts;
    }
    #endregion
}
