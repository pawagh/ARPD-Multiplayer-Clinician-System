using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class VRFrameController : NetworkBehaviour
{
    public Slider startFrameSlider;
    public Slider endFrameSlider;
    public Button playPauseButton;
    public TextMeshProUGUI playPauseButtonText;
    [SerializeField] private IKRigController ikRigController;
    [SerializeField] private TextMeshProUGUI frameRangeText;
    [SerializeField] private JSONLoader jsonLoader;

    [Networked] private NetworkBool IsPlaying { get; set; }
    [Networked] private float PlaybackSpeed { get; set; } = 30f; // Frames per second
    [Networked] private int CurrentFrame { get; set; }

    private TickTimer playbackTimer;
    private int totalFrames;

    public override void Spawned()
    {
        totalFrames = jsonLoader.GetFrameCount();
        InitializeUI();
        UpdateFrameRangeText();
    }

    private void InitializeUI()
    {
        startFrameSlider.minValue = 0;
        startFrameSlider.maxValue = totalFrames - 1;
        startFrameSlider.value = 0;
        startFrameSlider.onValueChanged.AddListener(OnStartSliderChanged);

        endFrameSlider.minValue = 0;
        endFrameSlider.maxValue = totalFrames - 1;
        endFrameSlider.value = totalFrames - 1;
        endFrameSlider.onValueChanged.AddListener(OnEndSliderChanged);

        playPauseButton.onClick.AddListener(OnPlayPauseButtonPressed);
        UpdatePlayPauseButtonText();
    }

    private void OnStartSliderChanged(float value)
    {
        if (Object.HasInputAuthority)
        {
            RPC_UpdateStartSliderValue(Mathf.RoundToInt(value));
        }
    }

    private void OnEndSliderChanged(float value)
    {
        if (Object.HasInputAuthority)
        {
            RPC_UpdateEndSliderValue(Mathf.RoundToInt(value));
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            UpdateFrameRangeText();

            if (IsPlaying)
            {
                UpdatePlayback();
            }
        }
    }

    private void UpdateFrameRangeText()
    {
        if (frameRangeText != null)
        {
            string newText = $"Frame Range: {Mathf.RoundToInt(startFrameSlider.value)} - {Mathf.RoundToInt(endFrameSlider.value)} / {totalFrames - 1}";
            frameRangeText.text = newText;
        }
    }

    private void UpdatePlayback()
    {
        if (playbackTimer.ExpiredOrNotRunning(Runner))
        {
            playbackTimer = TickTimer.CreateFromSeconds(Runner, 1f / PlaybackSpeed);
            CurrentFrame++;
            if (CurrentFrame > endFrameSlider.value)
            {
                CurrentFrame = Mathf.RoundToInt(startFrameSlider.value);
            }
            UpdateSkeletonWithCurrentFrame();
        }
    }

    private void UpdateSkeletonWithCurrentFrame()
    {
        if (CurrentFrame >= 0 && CurrentFrame < totalFrames)
        {
            ikRigController.UpdateSkeleton(CurrentFrame);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_TogglePlayPause()
    {
        IsPlaying = !IsPlaying;
        if (IsPlaying)
        {
            CurrentFrame = Mathf.RoundToInt(startFrameSlider.value);
            UpdateSkeletonWithCurrentFrame();
        }
        UpdatePlayPauseButtonText();
    }

    public void OnPlayPauseButtonPressed()
    {
        if (Object.HasInputAuthority)
        {
            RPC_TogglePlayPause();
        }
    }

    private void UpdatePlayPauseButtonText()
    {
        playPauseButtonText.text = IsPlaying ? "Pause" : "Play";
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateStartSliderValue(int newValue)
    {
        startFrameSlider.value = Mathf.Clamp(newValue, 0, endFrameSlider.value);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateEndSliderValue(int newValue)
    {
        endFrameSlider.value = Mathf.Clamp(newValue, startFrameSlider.value, totalFrames - 1);
    }
}