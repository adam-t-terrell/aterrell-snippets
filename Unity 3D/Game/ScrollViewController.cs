using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField] GameObject messageItem;
    private RectTransform content;
    private Scrollbar scrollbar;
    private ScrollRect scrollRect;

    private Stack<GameObject> messages = new Stack<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        content = this.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
        scrollbar = this.transform.Find("Scrollbar Vertical").GetComponent<Scrollbar>();
        scrollRect = this.GetComponent<ScrollRect>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMessage(string messageText)
    {
        PushMessage(messageText);
    }

    public void AddMessage(string messageText, Color color)
    {
        GameObject message = PushMessage(messageText);
        Image image = message.GetComponent<Image>();
        image.color = color;
    }

    private GameObject PushMessage(string messageText)
    {
        GameObject message = Instantiate(messageItem, content.transform);
        TextMeshProUGUI text = message.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        RectTransform transform = message.GetComponent<RectTransform>();

        text.text = messageText;
        transform.position = transform.position - (new Vector3(0, messages.Count * (transform.rect.height + 2), 0));

        messages.Push(message);
        content.sizeDelta = new Vector2(content.sizeDelta.x, messages.Count * (transform.rect.height + 2));

        scrollRect.verticalNormalizedPosition = 0;

        return message;
    }

    public void RemoveMessage()
    {
        GameObject message = messages.Pop();
        RectTransform transform = message.GetComponent<RectTransform>();

        content.sizeDelta = new Vector2(content.sizeDelta.x, messages.Count * (transform.rect.height + 2));
        Destroy(message);
    }
}