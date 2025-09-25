using UltimateFramework.SerializationSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace UltimateFramework.UISystem
{
    public class ExitConditionalUIElement : MonoBehaviour
    {
        [SerializeField] private int sceneID;
        private Action OnClick;

        private void Start()
        {
            if (TryGetComponent(out Button button))
            {
                if (DataGameManager.Instance != null)
                {
                    OnClick += DataGameManager.Instance.ExitGame;
                    button.onClick.AddListener(OnClick.Invoke);
                }
            }
        }
    }
}
