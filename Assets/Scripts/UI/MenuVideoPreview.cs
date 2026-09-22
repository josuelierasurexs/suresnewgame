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
            videoPlayer.Prepare();
        }

        private void OnPrepared(VideoPlayer preparedPlayer)
        {
            if (aspectFitter != null && preparedPlayer.width > 0 && preparedPlayer.height > 0)
                aspectFitter.aspectRatio = (float)preparedPlayer.width / preparedPlayer.height;
            preparedPlayer.Play();
        }

        private void OnEnable()
        {
            if (videoPlayer != null && videoPlayer.isPrepared && !videoPlayer.isPlaying)
                videoPlayer.Play();
        }

        private void OnDisable()
        {
            if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Pause();
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
