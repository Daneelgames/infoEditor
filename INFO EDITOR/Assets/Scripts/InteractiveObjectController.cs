using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InteractiveObjectController : MonoBehaviour
{
    [System.Serializable]
    public class Lists
    {
        public List<int> list;
    }

    public double maximumRating = 0.0; //strong = 1; weak = 0.1;
    public double minimalRating = 0.0;
    public double currentRating = 0.0;

    public enum Type { Book, Film, Photo };

    public List<Lists> wordsToCensorStrong;
    public List<Lists> wordsToCensorWeak;

    public List<Lists> wordsCensored = new List<Lists>();

    public bool interacting = false;
    public List<Outline> outlineControllers;
    public Transform camHolder;

    public List<TextMeshPro> pages = new List<TextMeshPro>();
    public TextMeshPro score;
    Camera cam;

    public GameObject censorMarkPrefab;
    public Image censorMarkTemplate;

    GameManager gm;
    private void Awake()
    {
        gm = GameManager.instance;
        gm.AddInteractiveObject(this);
    }

    private void Start()
    {
        score.text = "Score: " + currentRating + "/" + maximumRating;
        cam = Camera.main;
        ToggleOutline(false);

        //GET AVERAGE RATING
        foreach(Lists l in wordsToCensorStrong)
        {
            double pointsToAdd = l.list.Count;
            maximumRating += pointsToAdd;
            minimalRating += pointsToAdd;
        }
        foreach (Lists l in wordsToCensorWeak)
        {
            double pointsToAdd = 0.0;

            foreach(int i in l.list)
            {
                pointsToAdd += 0.1;
            }

            maximumRating += pointsToAdd;
        }
    }

    public void ToggleOutline(bool active)
    {
        foreach(Outline o in outlineControllers)
        {
            o.enabled = active;
        }
    }

    public void Interact()
    {
        //foreach (TextMeshPro page in pages)
        for (int i = 0; i < pages.Count; i ++)
        {
            int wordIndex = TMP_TextUtilities.FindIntersectingWord(pages[i], Input.mousePosition, cam);
            if (wordIndex >= 0)
            {
                /*
                string word = pages[i].textInfo.wordInfo[wordIndex].GetWord();
                string oldText = pages[i].text;
                string newText = oldText.Replace(word, "<mark=#000000>" + word + "</mark>");
                pages[i].textInfo.wordInfo[wordIndex].textComponent.SetText(newText);

                int characterIndex = pages[i].textInfo.wordInfo[wordIndex].firstCharacterIndex;
                TMP_CharacterInfo currentCharInfo = pages[i].textInfo.characterInfo[characterIndex];
                */

                if (wordsCensored[i].list.Count > 0)
                {
                    foreach (int c in wordsCensored[i].list)
                    {
                        if (c == wordIndex)
                        {
                            // skip everything if word already censored
                            return;
                        }
                    }
                }
                wordsCensored[i].list.Add(wordIndex);

                foreach (int iStrong in wordsToCensorStrong[i].list)
                {
                    if (iStrong == wordIndex)
                    {
                        currentRating += 1;
                        HighlightWord(pages[i], wordIndex, pages[i].transform);
                        return;
                    }
                }

                foreach (int iWeak in wordsToCensorWeak[i].list)
                {
                    if (iWeak == wordIndex)
                    {
                        currentRating += 0.1;
                        HighlightWord(pages[i], wordIndex, pages[i].transform);
                        return;
                    }
                }
                HighlightWord(pages[i], wordIndex, pages[i].transform);
            }
        }
    }


    void HighlightWord(TMP_Text m_TextComponent, int wordIndex, Transform m_Transform)
    {
        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        TMP_WordInfo wInfo = textInfo.wordInfo[wordIndex];

        bool isBeginRegion = false;

        Vector3 bottomLeft = Vector3.zero;
        Vector3 topLeft = Vector3.zero;
        Vector3 bottomRight = Vector3.zero;
        Vector3 topRight = Vector3.zero;

        float maxAscender = -Mathf.Infinity;
        float minDescender = Mathf.Infinity;

        Color wordColor = Color.green;

        // Iterate through each character of the word
        for (int j = 0; j < wInfo.characterCount; j++)
        {
            int characterIndex = wInfo.firstCharacterIndex + j;
            TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
            int currentLine = currentCharInfo.lineNumber;

            bool isCharacterVisible = characterIndex > m_TextComponent.maxVisibleCharacters ||
                                        currentCharInfo.lineNumber > m_TextComponent.maxVisibleLines ||
                                        (m_TextComponent.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != m_TextComponent.pageToDisplay) ? false : true;

            // Track Max Ascender and Min Descender
            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

            if (isBeginRegion == false && isCharacterVisible)
            {
                isBeginRegion = true;

                bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
                topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                // If Word is one character
                if (wInfo.characterCount == 1)
                {
                    isBeginRegion = false;

                    topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                    bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                    bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                    topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                    // Draw Region
                    CensorWord(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                }
            }

            // Last Character of Word
            if (isBeginRegion && j == wInfo.characterCount - 1)
            {
                isBeginRegion = false;

                topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                // Draw Region
                CensorWord(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
            }
            // If Word is split on more than one line.
            else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber)
            {
                isBeginRegion = false;

                topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                // Draw Region
                CensorWord(bottomLeft, topLeft, topRight, bottomRight, wordColor);
                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                maxAscender = -Mathf.Infinity;
                minDescender = Mathf.Infinity;
            }
        }
    }

    void CensorWord(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)
    {
        score.text = "Score: " + currentRating + "/" + maximumRating;
        GameObject newCensorMark = Instantiate(censorMarkPrefab, tl, censorMarkTemplate.transform.rotation);
        newCensorMark.transform.SetParent(transform);
        float scaleX = Vector3.Distance(tl, tr);
        float scaleZ = Vector3.Distance(tl, bl);
        newCensorMark.transform.localScale = new Vector3(scaleX, 1, scaleZ);
    }
}