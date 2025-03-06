using TMPro;
using UnityEngine;

public class TicTacToeUI : MonoBehaviour
{
    [SerializeField] private TicTacToeAI ticTacToeAI;

    [SerializeField] private TextMeshPro informationText;
    [SerializeField] private GameObject endGamePanel;

    private void Awake()
    {
        ticTacToeAI.OnWinner += OnWinner;
        ticTacToeAI.OnCurrentTurn += UpdateTurnText;
        ticTacToeAI.OnGameStarted += ResetUI;
    }

    private void OnDestroy()
    {
        ticTacToeAI.OnWinner -= OnWinner;
        ticTacToeAI.OnCurrentTurn -= UpdateTurnText;
        ticTacToeAI.OnGameStarted -= ResetUI;
    }

    private void UpdateTurnText(Turn currentTurn)
    {
        informationText.text = currentTurn == Turn.Player ? "Player's Turn" : "AI's Turn";
        informationText.color = currentTurn == Turn.Player ? Color.green : Color.red;
    }

    private void OnWinner(Winner winner)
    {
        Debug.Log("Winner: " + winner);

        switch (winner)
        {
            case Winner.Player:
                informationText.text = "Player Wins!";
                informationText.color = Color.green;
                break;
            case Winner.AI:
                informationText.text = "AI Wins!";
                informationText.color = Color.red;
                break;
            case Winner.Draw:
                informationText.text = "It's a Draw!";
                informationText.color = Color.yellow;
                break;
        }

        endGamePanel.SetActive(true);
    }

    private void ResetUI()
    {
        informationText.text = "Player's Turn";
        informationText.color = Color.green;
        endGamePanel.SetActive(false);
    }
}
