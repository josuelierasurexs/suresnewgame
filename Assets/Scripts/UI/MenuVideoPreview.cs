using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Surexs.DanceOff.UI
{
    public sealed class MenuVideoPreview : MonoBehaviour
    {
        private RawImage targetImage;
        private AspectRatioFitter aspectFitter;
        private VideoPlayer videoPlayer;
        private RenderTexture renderTexture;
        private bool shouldPlay;
        private bool prepareRequested;

        public void Configure(RawImage image, VideoClip clip, AspectRatioFitter fitter)
        {
            targetImage = image;
            aspectFitter = fitter;
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.skipOnDrop = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.clip = clip;
            shouldPlay = clip != null;

            if (clip == null)
            {
                Debug.LogWarning("[MainMenu] No hay VideoClip asignado para el demo de gameplay.", this);
                return;
            }

            renderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32)
            {
                name = "Menu Gameplay Preview"
            };
            renderTexture.Create();
            targetImage.texture = renderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.prepareCompleted += OnPrepared;
            RequestPrepare();
        }

        private void OnPrepared(VideoPlayer preparedPlayer)
        {
            prepareRequested = false;
            if (aspectFitter != null && preparedPlayer.width > 0 && preparedPlayer.height > 0)
                aspectFitter.aspectRatio = (float)preparedPlayer.width / preparedPlayer.height;
            if (shouldPlay && isActiveAndEnabled) preparedPlayer.Play();
        }

        private void OnEnable()
        {
            ResumePreview();
        }

        private void OnDisable()
        {
            if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Pause();
            if (videoPlayer != null && !videoPlayer.isPrepared)
            {
                videoPlayer.Stop();
                prepareRequested = false;
            }
        }

        private void Update()
        {
            if (!shouldPlay || videoPlayer == null) return;
            if (!videoPlayer.isPrepared)
            {
                RequestPrepare();
                return;
            }
            if (!videoPlayer.isPlaying) videoPlayer.Play();
        }

        private void ResumePreview()
        {
            if (!shouldPlay || videoPlayer == null || videoPlayer.clip == null) return;
            if (videoPlayer.isPrepared)
            {
                videoPlayer.Play();
                return;
            }
            RequestPrepare();
        }

        private void RequestPrepare()
        {
            if (prepareRequested || videoPlayer == null || videoPlayer.clip == null) return;
            prepareRequested = true;
            videoPlayer.Prepare();
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.prepareCompleted -= OnPrepared;
                videoPlayer.Stop();
                videoPlayer.targetTexture = null;
            }

            if (targetImage != null) targetImage.texture = null;
            if (renderTexture == null) return;
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
