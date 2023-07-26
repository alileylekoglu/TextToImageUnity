using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TextToImage : MonoBehaviour
{
    public TMP_InputField inputField; // Assign in inspector
    public Button sendButton; // Assign in inspector
    public RawImage rawImage; // Assign in inspector
    public Auth auth; // Assign in inspector

    private const string Url = "https://api.stability.ai/v1/generation/stable-diffusion-xl-beta-v2-2-2/text-to-image";

    [System.Serializable]
    private class TextPrompt
    {
        public string text;
        public int weight;
    }

    [System.Serializable]
    private class RequestBody
    {
        public int width;
        public int height;
        public int steps;
        public int seed;
        public int cfg_scale;
        public int samples;
        public string style_preset;
        public TextPrompt[] text_prompts;
    }

    private void Start()
    {
        // Disable button until input is given
        sendButton.interactable = false;
        // Add listener to the button
        sendButton.onClick.AddListener(() => StartCoroutine(PostRequest(inputField.text)));
        // Add listener to the input field
        inputField.onValueChanged.AddListener((value) => { sendButton.interactable = !string.IsNullOrEmpty(value); });
    }

    private IEnumerator PostRequest(string prompt)
    {
        var body = new RequestBody
        {
            width = 512,
            height = 512,
            steps = 50,
            seed = 0,
            cfg_scale = 7,
            samples = 1,
            style_preset = "enhance",
            text_prompts = new[]
            {
                new TextPrompt
                {
                    text = prompt,
                    weight = 1
                }
            }
        };

        var requestBody = JsonUtility.ToJson(body);
        var request = new UnityWebRequest(Url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestBody)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + auth.apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request error: " + request.error);
        }
        else
        {
            var response = JsonUtility.FromJson<ResponseModel>(request.downloadHandler.text);
            var image = response.artifacts[0];
            var bytes = System.Convert.FromBase64String(image.base64);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            rawImage.texture = texture;
        }
    }
}

[System.Serializable]
public class ResponseModel
{
    public ArtifactModel[] artifacts;
}

[System.Serializable]
public class ArtifactModel
{
    public string base64;
    public int seed;
}
