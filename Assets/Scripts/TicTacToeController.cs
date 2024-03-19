using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeController : MonoBehaviour {

    [Tooltip("Player 1 is played by the AI")]
    [HideInInspector]
    public bool p1Ai = false;

    [Tooltip("Player 2 is played by the AI")]
    [HideInInspector]
    public bool p2Ai = true;

    [Tooltip("Using hard coded instructions for the first two moves to improve the speed")]
    [HideInInspector]
    public bool useShortcuts = true;

    [Tooltip("Visualize the AI algorithm step by step")]
    [HideInInspector]
    public bool visualizeAI = false;

    [Tooltip("Duration of each AI algorithm step in seconds")]
    [HideInInspector]
    public float algorithmStepDuration = 1;

    [SerializeField]
    private List<Button> buttons = new List<Button>();

    public delegate void OnGameOverDelegate(int win);
    public OnGameOverDelegate onGameOverDelegate;

    private bool turn; 
    private int fieldsLeft;
    private bool isGameOver = true;


    private int recursionScore;
    private int optimalScoreButtonIndex = -1;

    public void StartGame() {
        turn = Mathf.Round(UnityEngine.Random.Range(0, 1)) == 1;
        Reset();
    }

    private void EnableButtons(bool enabled, bool ignoreEmpty = false) {
        foreach (Button button in buttons) {
            // Do not reanable buttons that already were used
            if (!enabled || ignoreEmpty || IsFieldEmpty(button)) {
                button.interactable = enabled;
            }
        }
    }

    private bool IsFieldEmpty(Button button) {
        return GetText(button).text == "";
    }

    private Text GetText(Button button) {
        return button.GetComponentInChildren<Text>();
    }

    private bool SetMarkAndCheckForWin(Button button, bool colorate = false) {
        Text text = GetText(button);
        if (text.text != "") {
            return false;
        }
        text.text = turn ? "X" : "O";
        fieldsLeft--;

        return CheckForWin(text.text, colorate);
    }

    public void OnButtonClick(Button button) {
        if (isGameOver) {
            Reset();
            return;
        }
        if (fieldsLeft <= 0) {
            return;
        }

        if (SetMarkAndCheckForWin(button, true)) {
            Win(); 
        }
        button.interactable = false;

  
        if (fieldsLeft <= 0) {
            GameOverDraw();
        }


        turn = !turn;

 
        if (!isGameOver && fieldsLeft > 0 && IsAiTurn()) {
            StartCoroutine(AiTurnCoroutine());
        }
    }

    private bool IsAiTurn() 
    {
        return (turn && p1Ai) || (!turn && p2Ai);
    }

    private IEnumerator AiTurnCoroutine()
    {
        EnableButtons(false);
  
        IEnumerator minMaxEnumerator = MinMaxCoroutine(1);
        if (visualizeAI) 
        {
            
            yield return StartCoroutine(minMaxEnumerator);
        } 
        else 
        {
            
            while (minMaxEnumerator.MoveNext()) 
            {

            }
        }

        HideDepthAndScoreForAllButtons();

        Button button = buttons[optimalScoreButtonIndex]; 
        EnableButtons(true);
        OnButtonClick(button);
    }


    private IEnumerator MinMaxCoroutine(int depth) {
        if (CheckBaseCaseAndShortcuts()) 
        {
            yield break;
        }
        int currentBestScore = turn ? Int32.MinValue : Int32.MaxValue;
        int currentOptimalScoreButtonIndex = -1;


        int fieldIndex = 0;
        while (fieldIndex < buttons.Count) 
        {
            if (IsFieldFree(fieldIndex)) 
            {
                Button button = buttons[fieldIndex];
                int currentScore = 0;

                bool endRecursion = false;

 
                if (visualizeAI && algorithmStepDuration > 0) 
                {
                    yield return new WaitForSeconds(algorithmStepDuration);
                }
                SetDepth(button, depth);


                if (SetMarkAndCheckForWin(button)) 
                {

                    currentScore = (turn ? 1 : -1) * (10 - depth);
                    endRecursion = true;
                } 
                else if (fieldsLeft > 0) 
                {

                    turn = !turn; 

                    IEnumerator minMaxEnumerator = MinMaxCoroutine(depth + 1);
                    if (visualizeAI) 
                    {

                        yield return StartCoroutine(minMaxEnumerator);
                    } 
                    else 
                    {

                        while (minMaxEnumerator.MoveNext()) 
                        { 

                        }
                    }
                    currentScore = recursionScore;
                    turn = !turn; // Switch turns back
                }

                if ((turn && currentScore > currentBestScore) || (!turn && currentScore < currentBestScore)) 
                {
                    currentBestScore = currentScore;
                    currentOptimalScoreButtonIndex = fieldIndex;
                }

                if (visualizeAI) 
                {
                    SetScore(button, currentScore);
                }


                if (visualizeAI && algorithmStepDuration > 0) 
                {
                    yield return new WaitForSeconds(algorithmStepDuration);
                }


                GetText(button).text = "";
                if (visualizeAI) 
                {
                    HideDepthAndScore(button);
                }
                fieldsLeft++;

                if (endRecursion) 
                {
                    break;
                }
            }
            fieldIndex++;

        }

        recursionScore = currentBestScore;
        optimalScoreButtonIndex = currentOptimalScoreButtonIndex;

    }

    private void SetScore(Button button, int score) 
    {
        Text scoreText = button.transform.GetChild(2).GetComponent<Text>();
        scoreText.gameObject.SetActive(true);
        scoreText.text = score + " Points";
    }

    private void SetDepth(Button button, int depth) 
    {
        Text depthText = button.transform.GetChild(1).GetComponent<Text>();
        depthText.gameObject.SetActive(true);
        depthText.text = "Depth: " + depth;

        GetText(button).color = Color.gray;
    }

    private void HideDepthAndScore(Button button) 
    {
        button.transform.GetChild(1).gameObject.SetActive(false);
        button.transform.GetChild(2).gameObject.SetActive(false);
        GetText(button).color = Color.white;
    }

    private void HideDepthAndScoreForAllButtons() 
    {
        foreach (Button button in buttons) 
        {
            HideDepthAndScore(button);
        }
    }


    private bool CheckBaseCaseAndShortcuts() {
        if (fieldsLeft <= 0) 
        {
            recursionScore = 0;
            return true;
        }

        if (!useShortcuts) 
        {
            return false;
        }


        if (fieldsLeft == 9) 
        {
            RandomCorner();
            return true;
        }

        if (fieldsLeft == 8) 
        {

            if (!GetText(buttons[4]).text.Equals("")) 
            {
                RandomCorner();
            } else 
            { 
                optimalScoreButtonIndex = 4;
            }
            return true;
        }
        return false;
    }



    private bool CheckForWin(string mark, bool colorate = false) {
        if (fieldsLeft > 6) {
            return false;
        }


        if (CompareButtons(0, 1, 2, mark, colorate)
         || CompareButtons(3, 4, 5, mark, colorate)
         || CompareButtons(6, 7, 8, mark, colorate)


         || CompareButtons(0, 3, 6, mark, colorate)
         || CompareButtons(1, 4, 7, mark, colorate)
         || CompareButtons(2, 5, 8, mark, colorate)


         || CompareButtons(0, 4, 8, mark, colorate)
         || CompareButtons(6, 4, 2, mark, colorate)) {
            return true;
        }
        return false;
    }

    private bool CompareButtons(int ind1, int ind2, int ind3, string mark, bool colorate = false) {
        Text text1 = GetText(buttons[ind1]);
        Text text2 = GetText(buttons[ind2]);
        Text text3 = GetText(buttons[ind3]);
        bool equal = text1.text == mark
                  && text2.text == mark
                  && text3.text == mark;
        if (colorate && equal) {
            Color color = turn ? Color.green : Color.red;
            text1.color = color;
            text2.color = color;
            text3.color = color;
        }
        return equal;
    }



    private bool IsFieldFree(int index) => GetText(buttons[index]).text.Length == 0;


    private void Win() {


        isGameOver = true;
        EnableButtons(false);
        onGameOverDelegate?.Invoke(turn ? 0 : 1);
    }
    private void GameOverDraw() {


        isGameOver = true;
        EnableButtons(false);
        onGameOverDelegate?.Invoke(-1);
    }



    private int RandomCorner() {
        optimalScoreButtonIndex = (int)Mathf.Floor(UnityEngine.Random.Range(0, 4));
        if (optimalScoreButtonIndex == 1) {
            optimalScoreButtonIndex = 6;
        } else if (optimalScoreButtonIndex == 3) {
            optimalScoreButtonIndex = 8;
        }
        return optimalScoreButtonIndex;
    }




    [ContextMenu("Reset")]
    private void Reset() 
    {
        foreach (Button button in buttons) 
        {
            Text text = GetText(button);
            text.color = Color.white;
            text.text = "";
            button.interactable = true;
        }
        fieldsLeft = 9;
        isGameOver = false;
        if (IsAiTurn()) {
            StartCoroutine(AiTurnCoroutine());
        }
    }


    [ContextMenu("Set Depth Test Example")]
    private void SetDepthTestExample() 
    {
        turn = true;
        Reset();
        GetText(buttons[1]).text = "X"; buttons[1].interactable = false;
        GetText(buttons[5]).text = "X"; buttons[5].interactable = false;
        GetText(buttons[6]).text = "O"; buttons[6].interactable = false;
        GetText(buttons[7]).text = "O"; buttons[7].interactable = false;
        GetText(buttons[8]).text = "X"; buttons[8].interactable = false;

        turn = false;
        fieldsLeft = 4;
        StartCoroutine(AiTurnCoroutine());
    }
}
