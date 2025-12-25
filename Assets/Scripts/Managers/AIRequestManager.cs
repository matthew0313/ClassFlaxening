using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using JetBrains.Annotations;
using Unity.Netcode;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class AIRequestManager : MonoBehaviour
{
    const string api_key = "REDACTED";
    const string request_url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";
    public static AIRequestManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void GetEnglishQuiz(Action<EnglishQuiz> onRespond) => Timing.RunCoroutine(GettingEnglishQuiz(onRespond));
    IEnumerator<float> GettingEnglishQuiz(Action<EnglishQuiz> onRespond)
    {
        using(UnityWebRequest request = new(request_url + api_key, "POST"))
        {
            string prompt = "Generate an English quiz where the player fills one line of blank in a conversation. The conversation should include at least five exchanges and cover common topics such as greetings, hobbies, travel, or food. Ensure that the language used is appropriate for intermediate English learners, incorporating useful vocabulary and phrases. Format the response as a JSON object with a 'conversation' array, where each element contains 'role' (either 'Person A' or 'Person B') and 'message' (the dialogue text or _____ for the whole line if blank). Example:";
            prompt += "Person A: Hi Alex! Long time no see. How have you been?\nPerson B: Hey Sarah! I'm doing great, thanks for asking. And you?\nPerson A: I'm good, just busy with work. But I've been trying to find more time for my hobbies. I've really gotten into photography lately.\nPerson B: Oh, really? That's fascinating! What kind of photography do you do?\nPerson A: Mostly landscapes and cityscapes. I love capturing the beauty of different places. It's quite relaxing.\nPerson B: Sounds lovely! I'm quite a fan of hiking myself. There's nothing like being out in nature.\nPerson A: Hiking is wonderful! Have you been anywhere interesting recently for a hike?\nPerson B: Yes, last month I went to the mountains upstate. The views were breathtaking. It made me think about traveling abroad more.\nPerson A: Traveling abroad sounds amazing! Where would you like to go?\nPerson B: I've always dreamt of exploring Italy, especially for the food and history.\nPerson A: Italy has incredible cuisine! Have you ever tried making any Italian dishes at home?\nPerson B: _____\nPerson A: I see! Well, if you ever go to Italy, you must try the pasta carbonara in Rome. It's a game-changer!\nPerson B: I'll definitely keep that in mind! Maybe we could even plan a trip together sometime?\nPerson A: That's a great idea! Let's talk more soon.";
            string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return Timing.WaitUntilDone(request.SendWebRequest());

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
                onRespond?.Invoke(null);
            }
            else
            {
                GeminiResponse response = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);
                string text = response.candidates[0].content.parts[0].text;
                text = text.Substring(7, text.Length - 10);
                EnglishQuiz quiz = JsonConvert.DeserializeObject<EnglishQuiz>(text);
                onRespond?.Invoke(quiz);
            }
        }
    }
    public void GetQuizScore(EnglishQuiz quiz, string answer, Action<float> onRespond) => Timing.RunCoroutine(GettingQuizScore(quiz, answer, onRespond));
    IEnumerator<float> GettingQuizScore(EnglishQuiz quiz, string answer, Action<float> onRespond)
    {
        using (UnityWebRequest request = new(request_url + api_key, "POST"))
        {
            string prompt = "Score the player's answer to a blank filling english quiz. Score the answer based on how natural and grammatically correct it is. Quiz: ";
            foreach (var i in quiz.conversation)
            {
                prompt += $"{i.role}: {i.message}\n";
            }
            prompt += $" Player's answer: {answer}. Respond with only the score out of 10.";
            string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return Timing.WaitUntilDone(request.SendWebRequest());

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
                onRespond?.Invoke(0.0f);
            }
            else
            {
                GeminiResponse response = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);
                string text = response.candidates[0].content.parts[0].text;
                Debug.Log(text);
                onRespond?.Invoke(float.Parse(text));
            }
        }
    }
}
[Serializable]
public class GeminiResponse
{
    public List<Candidate> candidates;
    [Serializable]
    public struct Candidate
    {
        public Content content;
    }

    [Serializable]
    public struct Content
    {
        public List<Part> parts;
    }

    [Serializable]
    public struct Part
    {
        public string text;
    }
}
[Serializable]
public class EnglishQuiz : INetworkSerializable
{
    public List<ConversationLine> conversation;

    [Serializable]
    public struct ConversationLine
    {
        public string role;
        public string message;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(AIRequestManager))]
public class AIRequestManager_Editor : Editor
{
    EnglishQuiz currentQuiz;
    string answer;
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
                    string log = "";
                    foreach(var i in quiz.conversation)
                    {
                        log += $"{i.role}: {i.message}\n";
                    }
                    Debug.Log(log);
                    currentQuiz = quiz;
                }
            });
        }
        answer = EditorGUILayout.TextField("Answer", answer);
        if(GUILayout.Button("Submit Answer"))
        {
            target.GetQuizScore(currentQuiz, answer, (score) =>
            {
                Debug.Log($"Score: {score}/10");
            }); 
        }
    }
}
#endif