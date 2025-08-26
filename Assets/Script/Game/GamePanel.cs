using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZeroMessenger;

namespace Script
{
    public class GamePanel : ADMonoBehaviour
    {
        [ADDependencyInject] private GameManager _GameManager;
        
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI youwinloseText;
        [SerializeField] private Button _replayButton;

        private int _givenTime = 120;
        private bool isTimerRunning = false;
        private float currentTime;

        private bool _isEndGame = false;
        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();

            MessageBroker<OnStartedGame>.Default.Subscribe(x =>
            {
                _isEndGame = false;
                timerText.gameObject.SetActive(false);
                youwinloseText.gameObject.SetActive(false);
                _replayButton.gameObject.SetActive(false);
            }).AddTo(this);
            
            MessageBroker<OnFinishedAnimationSequence>.Default.Subscribe(x =>
            {
                timerText.gameObject.SetActive(true);
                StartTimer().Forget();
            }).AddTo(this);
            
            MessageBroker<IsWinGame>.Default.Subscribe(x =>
            {
                if (x.IsWin)
                {
                    youwinloseText.gameObject.SetActive(true);
                    _replayButton.gameObject.SetActive(true);
                    youwinloseText.text = "You Win!";
                    _isEndGame = true;
                }
                else
                {
                    youwinloseText.gameObject.SetActive(true);
                    _replayButton.gameObject.SetActive(true);
                    youwinloseText.text = "You Lose!";
                    _isEndGame = true;
                }
            }).AddTo(this);
            
            _replayButton.onClick.AddListener(() =>
            {
                MessageBroker<OnResetingGame>.Default.Publish(new OnResetingGame());
            });
        }
        
        private async UniTask StartTimer()
        {
            currentTime = _givenTime;
            isTimerRunning = true;

            while (currentTime >= 0 && isTimerRunning)
            {
                if (_isEndGame)
                {
                    isTimerRunning = false;
                    break;
                }

                timerText.text = FormatTime(currentTime);
                await UniTask.Delay(1000);
                currentTime--;
            }

            if (isTimerRunning && !_isEndGame)
            {
                _GameManager.Lose();
                isTimerRunning = false;
            }
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
    }
}

public struct OnResetingGame
{
    
}