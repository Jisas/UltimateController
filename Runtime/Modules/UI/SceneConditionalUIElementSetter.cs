using UnityEngine.SceneManagement;
using UnityEngine;

namespace UltimateFramework
{
    public class SceneConditionalUIElementSetter : MonoBehaviour
    {
        [SerializeField] private int sceneID;
        [SerializeField] private GameObject[] elements;

        private void Awake()
        {
            if (SceneManager.GetActiveScene().buildIndex == sceneID)
            {
                foreach (GameObject element in elements)
                    element.SetActive(true);
            }
            else
            {
                foreach (GameObject element in elements)
                    element.SetActive(false);
            }
        }
    }
}
