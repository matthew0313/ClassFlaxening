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
    const string request_url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-pro-preview:generateContent?key=";
    public static AIRequestManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void GetEnglishQuiz(Action<string> onRespond) => Timing.RunCoroutine(GettingEnglishQuiz(onRespond));
    IEnumerator<float> GettingEnglishQuiz(Action<string> onRespond)
    {
        using(UnityWebRequest request = new(request_url + api_key, "POST"))
        {
            string prompt = "Generate an English quiz where the player fills one line of blank in a conversation. The conversation should include at least five exchanges and cover common topics such as greetings, hobbies, travel, or food. Ensure that the language used is appropriate for intermediate English learners, incorporating useful vocabulary and phrases. Format the response as a JSON object with a 'conversation' array, where each element contains 'role' (either 'Person A' or 'Person B') and 'message' (the dialogue text or _____ for the whole line if blank). Example:";
            prompt += "Person A: Hi Alex! Long time no see. How have you been?\nPerson B: Hey Sarah! I'm doing great, thanks for asking. And you?\nPerson A: I'm good, just busy with work. But I've been trying to find more time for my hobbies. I've really gotten into photography lately.\nPerson B: Oh, really? That's fascinating! What kind of photography do you do?\nPerson A: Mostly landscapes and cityscapes. I love capturing the beauty of different places. It's quite relaxing.\nPerson B: Sounds lovely! I'm quite a fan of hiking myself. There's nothing like being out in nature.\nPerson A: Hiking is wonderful! Have you been anywhere interesting recently for a hike?\nPerson B: Yes, last month I went to the mountains upstate. The views were breathtaking. It made me think about traveling abroad more.\nPerson A: Traveling abroad sounds amazing! Where would you like to go?\nPerson B: I've always dreamt of exploring Italy, especially for the food and history.\nPerson A: Italy has incredible cuisine! Have you ever tried making any Italian dishes at home?\nPerson B: _____\nPerson A: I see! Well, if you ever go to Italy, you must try the pasta carbonara in Rome. It's a game-changer!\nPerson B: I'll definitely keep that in mind! Maybe we could even plan a trip together sometime?\nPerson A: That's a great idea! Let's talk more soon.";
            var a = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };
            //string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";
            string json = JsonConvert.SerializeObject(a);
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
                string result = "";
                foreach(var i in quiz.conversation)
                {
                    result += $"{i.role}: {i.message}\n";
                }
                onRespond?.Invoke(result);
            }
        }
    }
    public void GetQuizScore(string quiz, List<string> answers, float maxScore, Action<List<float>> onRespond) => Timing.RunCoroutine(GettingQuizScore(quiz, answers, maxScore, onRespond));
    IEnumerator<float> GettingQuizScore(string quiz, List<string> answers, float maxScore, Action<List<float>> onRespond)
    {
        using (UnityWebRequest request = new(request_url + api_key, "POST"))
        {
            string prompt = $"Score the player's answer to a blank filling english quiz, out of {maxScore}. Score the answer based on how natural and grammatically correct it is. Quiz: {quiz},";
            for(int i = 0; i < answers.Count; i++)
            {
                prompt += $" Answer {i + 1}: {answers[i]},";
            }
            prompt += $"Respond with a JSON object. Don't add any unnecessary characters, not even ```json. respond with ONLY the following content inside:" +
                "   scores: [" +
                "       (score 1), (score 2), (score 3), (score 4), ..." +
                "   ]";
            var a = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };
            //string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";
            string json = JsonConvert.SerializeObject(a);
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
                Debug.Log(text);
                QuizResults results = JsonConvert.DeserializeObject<QuizResults>(text);
                onRespond?.Invoke(results.scores);
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
public class EnglishQuiz
{
    public List<ConversationLine> conversation;

    [Serializable]
    public struct ConversationLine
    {
        public string role;
        public string message;
    }
}
[Serializable]
public class QuizResults
{
    public List<float> scores = new();
}
#if UNITY_EDITOR
[CustomEditor(typeof(AIRequestManager))]
public class AIRequestManager_Editor : Editor
{
    string currentQuiz;
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
                    Debug.Log(quiz);
                    currentQuiz = quiz;
                }
            });
        }
        answer = EditorGUILayout.TextField("Answer", answer);
        if(GUILayout.Button("Submit Answer"))
        {
            List<string> answers = new();
            answers.Add(answer);
            target.GetQuizScore(currentQuiz, answers, 10.0f, (score) =>
            {
                Debug.Log($"Score: {score[0]}/10");
            }); 
        }
    }
}
#endif