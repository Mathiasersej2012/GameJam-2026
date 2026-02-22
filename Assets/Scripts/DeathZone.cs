using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string loserSceneName = "LOSER";
    [SerializeField] private float delayBeforeLoad = 1f;

    private bool _isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (_isLoading)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        _isLoading = true;
        StartCoroutine(LoadLoserSceneAfterDelay());
    }

    private IEnumerator LoadLoserSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(loserSceneName);
    }
}
