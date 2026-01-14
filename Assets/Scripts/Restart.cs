using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnMovement : MonoBehaviour
{
    [SerializeField] private float movementThreshold = 0.1f;

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        float diffX = Mathf.Abs(transform.position.x - _startPosition.x);
        float diffY = Mathf.Abs(transform.position.y - _startPosition.y);
        float diffZ = Mathf.Abs(transform.position.z - _startPosition.z);

        if (diffX > movementThreshold || diffY > movementThreshold || diffZ > movementThreshold)
        {
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}