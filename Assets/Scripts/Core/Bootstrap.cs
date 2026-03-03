using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kwiztime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string firstScene = "MainMenu";

        private void Start()
        {
            SceneManager.LoadScene(firstScene);
        }
    }
}