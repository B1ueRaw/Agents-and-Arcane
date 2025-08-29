using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageManager : MonoBehaviour
{
    public int maxMessages = 25;

    public GameObject chatPanel, textObject;
    public ScrollRect scrollRect;
    [SerializeField]
    List<Message> messages = new List<Message>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Space)){
        //     AddMessage("Archer: I am an archer, I shoot arrows, I am the best!");
        // }
    }

    public void AddMessage(string text){
        if(messages.Count >= maxMessages){
            Destroy(messages[0].textObject.gameObject);
            messages.Remove(messages[0]);
        }
        Message newMessage = new Message();
        newMessage.text = text;
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TMP_Text>();
        newMessage.textObject.text = newMessage.text;
        messages.Add(newMessage);

        // Scroll to the bottom after adding a new message
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        // Wait for end of frame to ensure UI updates
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}

[System.Serializable]
public class Message{
    public string text;
    public TMP_Text textObject;
}