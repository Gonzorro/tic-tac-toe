using UnityEngine;

public class ClickTrigger : MonoBehaviour
{
    [SerializeField] private TicTacToeAI ticTacToeAI;

    [SerializeField] private int _myCoordX = 0;
    [SerializeField] private int _myCoordY = 0;

    [SerializeField] private bool canClick;

    private void Awake()
    {
        ticTacToeAI.OnGameStarted += InitializeTrigger;
        ticTacToeAI.OnWinner += DisableClick;
    }

    private void OnDestroy()
    {
        ticTacToeAI.OnGameStarted -= InitializeTrigger;
        ticTacToeAI.OnWinner -= DisableClick;
    }

    private void InitializeTrigger()
    {
        ticTacToeAI.RegisterTransform(_myCoordX, _myCoordY, this);
        canClick = true;
    }

    private void OnMouseDown()
    {
        if (canClick) ticTacToeAI.PlayerSelects(_myCoordX, _myCoordY);
    }

    private void DisableClick(Winner winner)
    {
        Debug.Log("cant click");
        canClick = false;
    }
}
