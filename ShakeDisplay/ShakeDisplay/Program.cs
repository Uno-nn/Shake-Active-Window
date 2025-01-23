using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.Wave;

class Program
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    static void Main(string[] args)
    {
        var deviceEnumerator = new MMDeviceEnumerator();
        var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var capture = new WasapiLoopbackCapture(defaultDevice);

        capture.DataAvailable += OnDataAvailable;
        capture.StartRecording();

        Console.WriteLine("Причина тряски?...");
        Console.ReadLine();

        capture.StopRecording();
    }

    private static void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        float maxVolume = 0;
        for (int i = 0; i < e.BytesRecorded; i += 4) 
        {
            float sample = BitConverter.ToSingle(e.Buffer, i);
            float volume = Math.Abs(sample);
            if (volume > maxVolume)
                maxVolume = volume;
        }

        if (maxVolume > 0.7f)
        {
            ShakeWindow(maxVolume);
        }
    }

    private static void ShakeWindow(float intensity)
    {
        IntPtr hWnd = GetForegroundWindow(); //ЫЫЫЫЫ актив виндов ГЫЫЫЫЫЫ
        if (hWnd == IntPtr.Zero)
            return;

        //Корды
        GetWindowRect(hWnd, out RECT rect);
        int originalX = rect.Left;
        int originalY = rect.Top;
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        Random rand = new Random();
        int shakeAmount = (int)(intensity * 10); //Интенсивность

        //Причина Тряски?
        for (int i = 0; i < 10; i++)
        {
            int offsetX = rand.Next(-shakeAmount, shakeAmount);
            int offsetY = rand.Next(-shakeAmount, shakeAmount);

            MoveWindow(hWnd, originalX + offsetX, originalY + offsetY, width, height, true);
            Thread.Sleep(10); //ДЛя плавности
        }

        MoveWindow(hWnd, originalX, originalY, width, height, true);
    }
}
