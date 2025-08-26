using AsyncGameObjectsDependency.Core;
using UnityEngine;
using UnityEngine.UI;
using ZeroMessenger;

namespace Script
{
    public class MainMenuPanel : ADMonoBehaviour
    {
        [SerializeField] private Button _playButton;

        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();
            _playButton.onClick.AddListener(PlayGame);
        }

        private void PlayGame()
        {
            this.gameObject.SetActive(false);
            MessageBroker<OnStartedGame>.Default.Publish(new OnStartedGame());
        }
    }
    
    public struct OnStartedGame {}
}