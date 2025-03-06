using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TicTacToeState { none, cross, circle }
public enum Winner { Player, AI, Draw }
public enum Turn { Player, AI }

public class TicTacToeAI : MonoBehaviour
{
    private int _gridSize = 3;
    private List<Vector2Int> _usedGrids = new List<Vector2Int>();
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private TicTacToeState[,] boardState;
    private ClickTrigger[,] _triggers;

    private int _aiLevel;
    private bool _isPlayerTurn = true;
    private TicTacToeState playerState = TicTacToeState.cross;
    private TicTacToeState aiState = TicTacToeState.circle;
    private bool gameOver;

    [SerializeField] private GameObject _xPrefab;
    [SerializeField] private GameObject _oPrefab;

    public Action OnGameStarted;
    public Action<Turn> OnCurrentTurn;
    public Action<Winner> OnWinner;

    public void StartAI(int AILevel)
    {
        _aiLevel = AILevel;
        StartGame();
    }

    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
    {
        if (_triggers == null)
            _triggers = new ClickTrigger[_gridSize, _gridSize];

        _triggers[myCoordX, myCoordY] = clickTrigger;
    }

    private void StartGame()
    {
        _triggers = new ClickTrigger[_gridSize, _gridSize];
        boardState = new TicTacToeState[_gridSize, _gridSize];
        _usedGrids.Clear();
        gameOver = false;
        _isPlayerTurn = true;
        OnGameStarted?.Invoke();
        OnCurrentTurn?.Invoke(Turn.Player);
    }

    public void PlayerSelects(int coordX, int coordY)
    {
        if (!_isPlayerTurn || !CheckForUsedGrid(coordX, coordY)) return;

        SetVisual(coordX, coordY, playerState);
        CheckForWinner(Turn.Player);
        if (gameOver) return;

        _isPlayerTurn = false;
        OnCurrentTurn?.Invoke(Turn.AI);
        StartCoroutine(AiTurn());
    }

    private IEnumerator AiTurn()
    {
        yield return new WaitForSeconds(0.5f);

        Vector2Int aiMove = GetAvailableMove();
        if (aiMove != new Vector2Int(-1, -1))
            AiSelects(aiMove.x, aiMove.y);

        if (gameOver) yield break;

        OnCurrentTurn?.Invoke(Turn.Player);
        _isPlayerTurn = true;
    }

    public void AiSelects(int coordX, int coordY)
    {
        if (!CheckForUsedGrid(coordX, coordY)) return;

        SetVisual(coordX, coordY, aiState);
        CheckForWinner(Turn.AI);
    }

    private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
    {
        boardState[coordX, coordY] = targetState;

        Instantiate(targetState == TicTacToeState.circle ?
            _oPrefab :
            _xPrefab,
            _triggers[coordX, coordY].transform.position, Quaternion.identity);
    }

    private bool CheckForUsedGrid(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (_usedGrids.Contains(position)) return false;

        _usedGrids.Add(position);
        return true;
    }

    private Vector2Int GetAvailableMove()
    {
        availableMoves.Clear();

        for (int x = 0; x < _gridSize; x++)
            for (int y = 0; y < _gridSize; y++)
                if (boardState[x, y] == TicTacToeState.none)
                    availableMoves.Add(new Vector2Int(x, y));

        if (_aiLevel == 0)
        {
            if (availableMoves.Count == 0)
                return new Vector2Int(-1, -1);

            return availableMoves[UnityEngine.Random.Range(0, availableMoves.Count)];
        }
        else
        {
            Vector2Int winningMove = FindBestMove(aiState); // To win
            if (winningMove != new Vector2Int(-1, -1))
                return winningMove;

            Vector2Int blockingMove = FindBestMove(playerState); //To Block
            if (blockingMove != new Vector2Int(-1, -1))
                return blockingMove;

            if (availableMoves.Contains(new Vector2Int(1, 1))) // Get Center
                return new Vector2Int(1, 1);

            return availableMoves[UnityEngine.Random.Range(0, availableMoves.Count)]; //Random Move
        }
    }

    private Vector2Int FindBestMove(TicTacToeState targetState)
    {
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                if (boardState[x, y] == TicTacToeState.none)
                {
                    boardState[x, y] = targetState;

                    if (HasWinningLine(targetState))
                    {
                        boardState[x, y] = TicTacToeState.none;
                        return new Vector2Int(x, y);
                    }

                    boardState[x, y] = TicTacToeState.none;
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private void CheckForWinner(Turn currentTurn)
    {
        TicTacToeState checkState = currentTurn == Turn.Player ? playerState : aiState;

        if (HasWinningLine(checkState))
        {
            OnWinner?.Invoke(currentTurn == Turn.Player ? Winner.Player : Winner.AI);
            gameOver = true;
            return;
        }

        if (_usedGrids.Count == _gridSize * _gridSize)
        {
            OnWinner?.Invoke(Winner.Draw);
            gameOver = true;
        }
    }

    private bool HasWinningLine(TicTacToeState checkState)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            if (boardState[i, 0] == checkState && boardState[i, 1] == checkState && boardState[i, 2] == checkState)
                return true;
            if (boardState[0, i] == checkState && boardState[1, i] == checkState && boardState[2, i] == checkState)
                return true;
        }

        if (boardState[0, 0] == checkState && boardState[1, 1] == checkState && boardState[2, 2] == checkState)
            return true;
        if (boardState[0, 2] == checkState && boardState[1, 1] == checkState && boardState[2, 0] == checkState)
            return true;

        return false;
    }
}
