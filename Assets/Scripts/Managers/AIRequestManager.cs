using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class AIRequestManager : MonoBehaviour
{
    const string api_key = "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";
    const string request_url = "https://api.openai.com/v1/responses";
    public static AIRequestManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void GetEnglishQuiz(Action<EnglishQuiz> onRespond) => Timing.RunCoroutine(GettingEnglishQuiz(onRespond).CancelWith(gameObject));
    IEnumerator<float> GettingEnglishQuiz(Action<EnglishQuiz> onRespond)
    {
        var prompt = new
        {
            model = "gpt-4o-mini",
            instructions = "You are an English conversation quiz generator.",
            input = new[]
            {
                new
                {
                    role = "user",
                    content = new[]
                    {
                        new
                        {
                            type = "input_text",
                            text = "Generate an english conversation, leaving one line of blank for the player to fill up. It'll be used as a quiz, which will be scored based how natural and grammatically correct the player's answer for the blank is."
                        }
                    }
                }
            },
            text = new
            {
                format = new
                {
                    name = "EnglishQuiz",
                    type = "json_schema",
                    schema = new
                    {
                        strict = true,
                        type = "object",
                        properties = new
                        {
                            conversation = new
                            {
                                type = "array",
                                minItems = 3,
                                maxItems = 5,
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        role = new
                                        {
                                            type = "string"
                                        },
                                        message = new
                                        {
                                            type = "string"
                                        }
                                    },
                                    required = new string[] { "role", "message" },
                                    additionalProperties = false
                                }
                            }
                        },
                        required = new string[] { "conversation" },
                        additionalProperties = false
                    }
                }
            }
        };
        UnityWebRequest request = new(request_url, "POST");
        string json = JsonConvert.SerializeObject(prompt, Formatting.Indented);
        Debug.Log(json);
        var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        uploadHandler.contentType = "application/json";
        request.uploadHandler = uploadHandler;
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", $"Bearer {api_key}");

        yield return Timing.WaitUntilDone(request.SendWebRequest());

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
            onRespond?.Invoke(null);
        }
        else
        {
            EnglishQuiz quiz = JsonUtility.FromJson<EnglishQuiz>(request.downloadHandler.text);
            onRespond?.Invoke(quiz);
        }
    }
    public void GetQuizScore(EnglishQuiz quiz, string answer, Action<bool, float> onRespond)
    {

    }
}
[System.Serializable]
public struct ConversationElement
{
    public string role;
    public string message;
}
[System.Serializable]
public class EnglishQuiz
{
    public List<ConversationElement> conversation;
}
[System.Serializable]
public class EnglishQuizScore
{
    public float naturalScore;
    public float grammarScore;
}
#if UNITY_EDITOR
[CustomEditor(typeof(AIRequestManager))]
public class AIRequestManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AIRequestManager target = (AIRequestManager)base.target;
        if (GUILayout.Button("Test Get English Quiz"))
        {
            target.GetEnglishQuiz((quiz) =>
            {
                if (quiz != null)
                {
                    string content = "Received Quiz:\n";
                    foreach (var element in quiz.conversation)
                    {
                        content += $"{element.role}: {element.message}\n";
                    }
                    Debug.Log(content);
                }
            });
        }
    }
}
#endif