using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using NAudio.Wave;
using Vosk;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KeyboardInput ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
        public uint Padding1;
        public uint Padding2;
    }

    private const uint InputKeyboard = 1;

    static void Main(string[] args)
    {
        string configFilePath = "config.json";
        string modelPath = GetModelPathFromConfig(configFilePath);

        var model = new Model(modelPath);

        using var recognizer = new VoskRecognizer(model, 16000.0f);
        using var waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(16000, 1);

        waveIn.DataAvailable += (sender, e) =>
        {
            if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var resultJson = recognizer.Result();
                var resultText = ParseResultText(resultJson);
                switch (resultText)
                {
                    case "comma":
                        SendComma();
                        break;
                    case "period":
                        SendPeriod();
                        break;
                    case "new line":
                        SendNewline();
                        break;
                    case "send message":
                        SendMessage();
                        break;
                    default:
                        SendTextToActiveWindow(resultText);
                        break;
                }
            }
            else
            {
                var partialJson = recognizer.PartialResult();
                var partialText = ParsePartialText(partialJson);
                // Optional: Send partial results if needed
                // SendTextToActiveWindow(partialText);
            }
        };

        waveIn.StartRecording();

        Console.WriteLine("Listening... Press Enter to stop.");
        Console.ReadLine();

        waveIn.StopRecording();
        Console.WriteLine("Final result:");
        Console.WriteLine(recognizer.FinalResult());
    }

    static string GetModelPathFromConfig(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException("Configuration file not found: " + configFilePath);
        }

        string jsonContent = File.ReadAllText(configFilePath);
        var config = JObject.Parse(jsonContent);
        return config["modelPath"]?.ToString() ?? throw new Exception("Model path not found in config file.");
    }

    static string ParseResultText(string json)
    {
        var jsonObj = JObject.Parse(json);
        return jsonObj["text"]?.ToString() ?? string.Empty;
    }

    static string ParsePartialText(string json)
    {
        var jsonObj = JObject.Parse(json);
        return jsonObj["partial"]?.ToString() ?? string.Empty;
    }

    static void SendTextToActiveWindow(string text)
    {
        if (text.Length > 0)
        {
            Console.WriteLine(text);
            foreach (char c in text)
            {
                SendKey(c);
                System.Threading.Thread.Sleep(10);  // Adjust the delay as needed
            }
            // Add a space after the text (if needed)
            SendKey(' ');
        }
    }

    static void SendKey(char key)
    {
        var inputs = new Input[1];
        inputs[0].Type = InputKeyboard;
        inputs[0].U.ki = new KeyboardInput
        {
            Vk = 0,
            Scan = (ushort)key,
            Flags = 0x0004,  // KEYEVENTF_UNICODE
            Time = 0,
            ExtraInfo = IntPtr.Zero
        };

        SendInput(1, inputs, Marshal.SizeOf(typeof(Input)));
    }

    static void SendComma() => SendKey(',');
    static void SendPeriod() => SendKey('.');
    static void SendNewline() => SendKey('\n');
    static void SendMessage()
    {
        var inputs = new Input[1];
        inputs[0].Type = InputKeyboard;
        inputs[0].U.ki = new KeyboardInput
        {
            Vk = 0x0D,  // VK_RETURN (Enter key)
            Scan = 0,
            Flags = 0,  // No special flags for key press
            Time = 0,
            ExtraInfo = IntPtr.Zero
        };

        SendInput(1, inputs, Marshal.SizeOf(typeof(Input)));

        // Optional: Add a key-up event to simulate key release
        inputs[0].U.ki.Flags = 0x0002;  // KEYEVENTF_KEYUP flag
        SendInput(1, inputs, Marshal.SizeOf(typeof(Input)));
    }
}
