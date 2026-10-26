using Engine;
using Engine.Graphics;
using Engine.Media;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Game;

namespace ZanJhat.Core
{
    public static class CinematicRecorderManager
    {
        public static string CinematicDirectory => PathManager.CinematicDirectory;
        public static bool IsRecording => m_isRecording;

        public static ComponentPlayer RecordingPlayer { get; private set; }

        private static bool m_isRecording = false;
        private static RenderTarget2D m_renderTarget;
        private static int m_frameCounter;
        private static string m_currentSessionFolder;
        private static CancellationTokenSource m_cancelToken;

        private struct FrameData
        {
            public Image PixelData;
            public string FilePath;
        }

        private static ConcurrentQueue<FrameData> m_frameQueue = new();

        public static bool StartRecording(ComponentPlayer componentPlayer, int width, int height)
        {
            if (m_isRecording || componentPlayer == null)
                return false;

            RecordingPlayer = componentPlayer;

            m_renderTarget = new RenderTarget2D(width, height, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);
            m_frameCounter = 0;

            DateTime now = DateTime.Now;
            m_currentSessionFolder = Storage.CombinePaths(CinematicDirectory, $"Session_{now:yyyyMMdd_HHmmss}");
            Storage.CreateDirectory(m_currentSessionFolder);

            m_isRecording = true;

            m_cancelToken = new CancellationTokenSource();
            Task.Run(() => SaveFramesWorker(m_cancelToken.Token));

            return true;
        }

        public static bool StopRecording(ComponentPlayer requestor = null)
        {
            if (!m_isRecording)
                return false;

            if (requestor != null && requestor != RecordingPlayer) return false;

            m_isRecording = false;
            RecordingPlayer = null;

            // Chỉ báo hiệu dừng, KHÔNG Dispose m_renderTarget ở đây để tránh lỗi ảnh cuối
            m_cancelToken?.Cancel();

            return true;
        }

        // FIX 1: Nhận thêm ComponentPlayer để đánh lừa Camera
        public static void RecordFrame(Camera cinematicCamera, ComponentPlayer targetPlayer)
        {
            if (!m_isRecording || m_renderTarget == null) return;

            RenderTarget2D originalTarget = Display.RenderTarget;
            Camera originalCamera = targetPlayer.GameWidget.ActiveCamera;

            try
            {
                Display.RenderTarget = m_renderTarget;
                Display.Clear(Color.Black, 1f, 0);

                // TRICK: Ép game tin rằng Camera quay phim đang là Camera chính
                // Nhờ đó, thân hình (Model) của người chơi sẽ được vẽ ra!
                targetPlayer.GameWidget.ActiveCamera = cinematicCamera;

                GameManager.Project.FindSubsystem<SubsystemDrawing>(true).Draw(cinematicCamera);

                Image frameImage = m_renderTarget.GetData(new Rectangle(0, 0, m_renderTarget.Width, m_renderTarget.Height));

                // ĐỔI SANG JPG: Nhanh hơn gấp 10 lần WebP và dung lượng cực nhẹ (~100-300kb)
                string filename = $"frame_{m_frameCounter:D5}.jpg";
                m_frameQueue.Enqueue(new FrameData
                {
                    PixelData = frameImage,
                    FilePath = Storage.CombinePaths(m_currentSessionFolder, filename)
                });

                m_frameCounter++;
            }
            finally
            {
                // Trả lại mọi thứ như cũ
                targetPlayer.GameWidget.ActiveCamera = originalCamera;
                Display.RenderTarget = originalTarget;
            }
        }

        private static void SaveFramesWorker(CancellationToken token)
        {
            while (!token.IsCancellationRequested || !m_frameQueue.IsEmpty)
            {
                if (m_frameQueue.TryDequeue(out FrameData frame))
                {
                    try
                    {
                        using (Stream stream = Storage.OpenFile(frame.FilePath, OpenFileMode.Create))
                        {
                            // Lưu bằng định dạng JPG để gánh được 30FPS mà không bị phình RAM
                            Image.Save(frame.PixelData, stream, ImageFileFormat.Jpg, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Cinematic Recorder Save Error: {ex.Message}");
                    }
                }
                else
                {
                    Thread.Sleep(5);
                }
            }

            // FIX 2: Dọn dẹp RenderTarget sau khi Hàng đợi đã lưu xong tấm ảnh cuối cùng
            m_renderTarget?.Dispose();
            m_renderTarget = null;
        }
    }
}
