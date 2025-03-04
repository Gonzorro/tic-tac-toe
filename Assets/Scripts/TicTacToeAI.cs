using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public enum TicTacToeState { none, cross, circle }

[System.Serializable]
public class WinnerEvent : UnityEvent<int> { }

public class TicTacToeAI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerText;
    int _aiLevel;
    TicTacToeState[,] boardState;
    [SerializeField] private bool _isPlayerTurn;
    [SerializeField] private int _gridSize = 3;
    [SerializeField] private TicTacToeState playerState = TicTacToeState.circle;
    [SerializeField] private TicTacToeState aiState = TicTacToeState.cross;
    [SerializeField] private GameObject _xPrefab;
    [SerializeField] private GameObject _oPrefab;
    public UnityEvent onGameStarted;
    public WinnerEvent onPlayerWin;
    ClickTrigger[,] _triggers;

    private void Awake()
    {
        if (onPlayerWin == null)
            onPlayerWin = new WinnerEvent();
    }

    public void StartAI(int AILevel)
    {
        _aiLevel = AILevel;
        StartGame();
    }

    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger) =>
        _triggers[myCoordX, myCoordY] = clickTrigger;

    private void StartGame()
    {
        _triggers = new ClickTrigger[_gridSize, _gridSize];
        boardState = new TicTacToeState[_gridSize, _gridSize];
        for (int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
                boardState[i, j] = TicTacToeState.none;
        _isPlayerTurn = true;
        onGameStarted.Invoke();
    }

    public void PlayerSelects(int coordX, int coordY)
    {
        if (_isPlayerTurn && boardState[coordX, coordY] == TicTacToeState.none)
        {
            boardState[coordX, coordY] = playerState;
            SetVisual(coordX, coordY, playerState);
            if (CheckGameOver()) return;
            _isPlayerTurn = false;
            AiMove();
        }
    }

    public void AiSelects(int coordX, int coordY)
    {
        boardState[coordX, coordY] = aiState;
        SetVisual(coordX, coordY, aiState);
        if (CheckGameOver()) return;
        _isPlayerTurn = true;
    }

    private void SetVisual(int coordX, int coordY, TicTacToeState targetState) =>
        Instantiate(targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
            _triggers[coordX, coordY].transform.position, Quaternion.identity);

    private void AiMove()
    {
        var availableMoves = GetAvailableMoves(boardState);
        if (availableMoves.Count == 0) return;
        int selectedX = 0, selectedY = 0;
        if (_aiLevel == 0)
        {
            int randomIndex = Random.Range(0, availableMoves.Count);
            (selectedX, selectedY) = availableMoves[randomIndex];
        }
        else
        {
            int bestScore = int.MinValue;
            foreach (var move in availableMoves)
            {
                boardState[move.Item1, move.Item2] = aiState;
                int score = Minimax(boardState, 0, false);
                boardState[move.Item1, move.Item2] = TicTacToeState.none;
                if (score > bestScore)
                {
                    bestScore = score;
                    selectedX = move.Item1;
                    selectedY = move.Item2;
                }
            }
        }
        AiSelects(selectedX, selectedY);
    }

    List<(int, int)> GetAvailableMoves(TicTacToeState[,] board)
    {
        var moves = new List<(int, int)>();
        for (int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
                if (board[i, j] == TicTacToeState.none)
                    moves.Add((i, j));
        return moves;
    }

    int Minimax(TicTacToeState[,] board, int depth, bool isMaximizing)
    {
        var winner = CheckWinner(board);
        if (winner == aiState)
            return 10 - depth;
        if (winner == playerState)
            return depth - 10;
        var moves = GetAvailableMoves(board);
        if (moves.Count == 0)
            return 0;
        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            foreach (var move in moves)
            {
                board[move.Item1, move.Item2] = aiState;
                int score = Minimax(board, depth + 1, false);
                board[move.Item1, move.Item2] = TicTacToeState.none;
                bestScore = score > bestScore ? score : bestScore;
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            foreach (var move in moves)
            {
                board[move.Item1, move.Item2] = playerState;
                int score = Minimax(board, depth + 1, true);
                board[move.Item1, move.Item2] = TicTacToeState.none;
                bestScore = score < bestScore ? score : bestScore;
            }
            return bestScore;
        }
    }

    TicTacToeState CheckWinner(TicTacToeState[,] board)
    {
        for (int i = 0; i < _gridSize; i++)
            if (board[i, 0] != TicTacToeState.none && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                return board[i, 0];
        for (int j = 0; j < _gridSize; j++)
            if (board[0, j] != TicTacToeState.none && board[0, j] == board[1, j] && board[1, j] == board[2, j])
                return board[0, j];
        if (board[0, 0] != TicTacToeState.none && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            return board[0, 0];
        if (board[0, 2] != TicTacToeState.none && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            return board[0, 2];
        return TicTacToeState.none;
    }

    bool CheckGameOver()
    {
        var winner = CheckWinner(boardState);
        if (winner != TicTacToeState.none)
        {
            onPlayerWin.Invoke(winner == playerState ? 1 : 2);
            winnerText.text = winner == playerState ? "Player Wins!" : "AI Wins!";
            return true;
        }
        if (GetAvailableMoves(boardState).Count == 0)
        {
            onPlayerWin.Invoke(0);
            winnerText.text = "It's a Draw!";
            return true;
        }
        return false;
    }
}
