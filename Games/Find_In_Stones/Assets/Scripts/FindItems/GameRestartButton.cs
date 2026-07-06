using UnityEngine;
using UnityEngine.UI;

namespace FindInStones
{
    [RequireComponent(typeof(Button))]
    public class GameRestartButton : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Restart);
        }

        void Restart()
        {
            if (FindItemsGameController.Instance != null)
                FindItemsGameController.Instance.RestartGame();
        }
    }
}
