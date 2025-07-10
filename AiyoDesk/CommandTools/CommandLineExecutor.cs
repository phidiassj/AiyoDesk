using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tmds.DBus.Protocol;

namespace AiyoDesk.CommanandTools;

public class CommandLineExecutor : IDisposable
{
    public bool CondaEnvExists { get; private set; }
    public string MessageLogs { get; private set; } = string.Empty;

    private Process? _cmdProcess;
    private bool _disposed = false;

    private const int CTRL_C_EVENT = 0;
    private const int CTRL_BREAK_EVENT = 1;

    // 導入 Windows API
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    public static string GetAppRootPath()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public static string GetPackageRootPath()
    {
        return Path.Combine(GetAppRootPath(), "AppPackages");
    }

    public static string GetScriptRootPath()
    {
        return Path.Combine(GetAppRootPath(), "CommandTools");
    }

    public static string GetAIModelsPath()
    {
        return Path.Combine(GetAppRootPath(), "AIModels");
    }

    public static string GetCondaScriptPath()
    {
        return Path.Combine(GetPackageRootPath(), "conda", "envs", "aiyodesk", "Scripts");
    }

    public static void StartProcess(string commandString)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = commandString,
            UseShellExecute = true
        });
    }

    // 控制台控制處理器委託
    private delegate bool ConsoleCtrlDelegate(uint CtrlType);

    /// <summary>
    /// 每當有新的輸出行時觸發的事件
    /// </summary>
    public event EventHandler<string>? OutputReceived;

    private void newLog(string content)
    {
        if (!MessageLogs.EndsWith('\n')) MessageLogs += '\n';
        MessageLogs += content;
        OutputReceived?.Invoke(this, content);
    }

    /// <summary>
    /// 初始化一個新的 CommandLineExecutor 實例並啟動 cmd.exe
    /// </summary>
    public CommandLineExecutor()
    {
        InitializeProcess();
    }

    private void InitializeProcess()
    {
        _cmdProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false, 
                WindowStyle = ProcessWindowStyle.Minimized, 
                Arguments = "/K",
            },
            EnableRaisingEvents = true
        };

        _cmdProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                //OutputReceived?.Invoke(this, e.Data);
                newLog(e.Data);
            }
        };

        _cmdProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                //OutputReceived?.Invoke(this, e.Data);
                //OutputReceived?.Invoke(this, $"ERROR: {e.Data}");
                newLog(e.Data);
            }
        };

        _cmdProcess.Start();
        _cmdProcess.BeginOutputReadLine();
        _cmdProcess.BeginErrorReadLine();
    }

    /// <summary>
    /// 執行指定的命令列指令
    /// </summary>
    /// <param name="command">要執行的指令</param>
    public void ExecuteCommand(string command)
    {
        if (_cmdProcess == null || _cmdProcess.HasExited)
        {
            InitializeProcess();
        }

        _cmdProcess?.StandardInput.WriteLine(command);
        _cmdProcess?.StandardInput.Flush();
    }

    /// <summary>
    /// 執行命令並使用回調函數即時處理輸出
    /// </summary>
    /// <param name="command">要執行的命令</param>
    /// <param name="outputHandler">處理輸出的回調函數</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>當命令完成時完成的任務</returns>
    public async Task ExecuteCommandWithRealtimeOutputAsync(
        string command,
        Action<string> outputHandler,
        CancellationToken cancellationToken = default)
    {
        if (_cmdProcess == null || _cmdProcess.HasExited)
        {
            InitializeProcess();
        }

        var tcs = new TaskCompletionSource<bool>();
        var commandDone = false;
        string promptPattern = $"__CMD_DONE__{Guid.NewGuid()}";

        EventHandler<string>? handler = null;
        string lastLine = string.Empty;
        handler = (sender, output) =>
        {
            // 檢測命令是否完成(基於命令提示符的出現)
            if (!commandDone && output.TrimEnd().EndsWith(promptPattern))
            {
                commandDone = true;
                tcs.TrySetResult(true);
                ExecuteCommand("cls");
            }

            // 呼叫回調來即時處理輸出
            if (output != lastLine && !commandDone)
            {
                outputHandler?.Invoke(output);
                lastLine = output;
            }
        };

        try
        {
            OutputReceived += handler;
            // 註冊取消處理
            using (cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
                SendInterrupt(); // 當取消時發送中斷
            }))
            {
                // 執行命令
                ExecuteCommand(command);
                ExecuteCommand($"echo {promptPattern}");

                // 等待命令完成或取消
                await tcs.Task;
            }
        }
        catch (TaskCanceledException)
        {
            // 任務已被取消，這是預期的行為
        }
        finally
        {
            OutputReceived -= handler;
        }
    }

    /// <summary>
    /// 執行命令並以流的方式獲取輸出
    /// </summary>
    /// <param name="command">要執行的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>輸出行的非同步序列</returns>
    public async IAsyncEnumerable<string> ExecuteCommandAsStreamAsync(
        string command,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_cmdProcess == null || _cmdProcess.HasExited)
        {
            InitializeProcess();
        }

        var outputQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var outputAvailable = new SemaphoreSlim(0);
        var commandCompleted = new TaskCompletionSource<bool>();
        string promptPattern = $"__CMD_DONE__{Guid.NewGuid()}";

        EventHandler<string>? handler = null;
        handler = (sender, output) =>
        {
            if (output.TrimEnd().EndsWith(promptPattern) && !commandCompleted.Task.IsCompleted)
            {
                commandCompleted.TrySetResult(true);
            }
            else
            {
                outputQueue.Enqueue(output);
                outputAvailable.Release();
            }
        };

        try
        {
            OutputReceived += handler;

            // 註冊取消處理
            using (cancellationToken.Register(() =>
            {
                commandCompleted.TrySetCanceled();
                SendInterrupt(); // 當取消時發送中斷
            }))
            {
                // 執行命令
                ExecuteCommand(command);
                ExecuteCommand($"echo {promptPattern}");

                // 持續讀取輸出，直到命令完成或取消
                while (!commandCompleted.Task.IsCompleted || !outputQueue.IsEmpty)
                {
                    // 等待新的輸出或完成
                    if (outputQueue.IsEmpty && !commandCompleted.Task.IsCompleted)
                    {
                        try
                        {
                            await Task.WhenAny(
                                outputAvailable.WaitAsync(cancellationToken),
                                commandCompleted.Task
                            ).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break; // 取消操作
                        }
                    }

                    // 嘗試從隊列獲取輸出
                    if (outputQueue.TryDequeue(out var line))
                    {
                        yield return line;
                    }
                }
            }
        }
        finally
        {
            OutputReceived -= handler;
        }
    }

    /// <summary>
    /// 發送組合鍵到命令行窗口
    /// </summary>
    /// <param name="controlKey">要發送的控制鍵組合</param>
    public void SendControlKey(ControlKeys controlKey)
    {
        switch (controlKey)
        {
            case ControlKeys.CtrlC:
                SendInterrupt();
                break;

            case ControlKeys.CtrlD:
                if (_cmdProcess != null && !_cmdProcess.HasExited)
                {
                    _cmdProcess.StandardInput.Write('\u0004'); // Ctrl+D (EOT)
                    _cmdProcess.StandardInput.Flush();
                }
                break;

            case ControlKeys.CtrlZ:
                if (_cmdProcess != null && !_cmdProcess.HasExited)
                {
                    _cmdProcess.StandardInput.Write('\u001A'); // Ctrl+Z (SUB)
                    _cmdProcess.StandardInput.Flush();
                }
                break;

            default:
                throw new ArgumentException($"不支援的控制鍵組合: {controlKey}");
        }
    }

    /// <summary>
    /// 使用 Windows API 發送 Ctrl+C 信號
    /// </summary>
    private void SendCtrlCViaWindowsApi()
    {
        if (_cmdProcess == null) return;
        try
        {
            // 將當前進程附加到目標進程的控制台
            if (AttachConsole((uint)_cmdProcess.Id))
            {
                // 禁用這個進程的 Ctrl+C 處理，這樣 Ctrl+C 就只會影響目標進程
                SetConsoleCtrlHandler(null, true);

                // 發送 Ctrl+C 信號
                GenerateConsoleCtrlEvent(CTRL_C_EVENT, (uint)_cmdProcess.Id);

                // 重新啟用 Ctrl+C 處理
                SetConsoleCtrlHandler(null, false);

                // 分離控制台
                FreeConsole();
            }
        }
        catch (Exception ex)
        {
            string msg = $"ERROR: 發送 Ctrl+C 失敗: {ex.Message}";
            OutputReceived?.Invoke(this, msg);
            newLog(msg);
        }
    }

    /// <summary>
    /// 發送 Ctrl+C 中斷當前執行的指令
    /// </summary>
    public void SendInterrupt()
    {
        //SendControlKey(ControlKeys.CtrlC);
        SendCtrlCViaWindowsApi();
    }

    /// <summary>
    /// 執行指令並等待其完成，返回完整輸出
    /// </summary>
    /// <param name="command">要執行的指令</param>
    /// <returns>指令執行的輸出結果</returns>
    public async Task<string> ExecuteCommandAndWaitForOutputAsync(string command, CancellationToken cancellationToken = default)
    {
        var outputBuilder = new StringBuilder();

        await ExecuteCommandWithRealtimeOutputAsync(
            command,
            output => outputBuilder.AppendLine(output),
            cancellationToken
        );

        return outputBuilder.ToString();
    }

    private static int condaAcivateCount = 0;
    public async Task ActivateCondaEnv()
    {
        while(condaAcivateCount > 0)
        {
            await Task.Delay(500);
        }
        condaAcivateCount++;

        string execCommand = string.Empty;
        if (checkCondaInstalled())
        {
            string activatePath = Path.Combine(GetPackageRootPath(), "conda", "condabin", "activate.bat");
            string envPath = Path.Combine(GetPackageRootPath(), "conda", "envs", "aiyodesk");
            execCommand = $"{activatePath} \"{envPath}\"";
        }
        else
        {
            execCommand = "conda activate aiyodesk";
        }

        List<string> resultList = new();
        await ExecuteCommandWithRealtimeOutputAsync(execCommand, resultLine =>
        {
            resultLine = resultLine.TrimEnd('\n');
            if (!string.IsNullOrWhiteSpace(resultLine)) resultList.Add(resultLine);
        });

        condaAcivateCount--;
        await checkCondaEnvExists();
    }

    public async Task checkCondaEnvExists()
    {
        string execCommand = "conda --version";

        List<string> resultList = new();
        await ExecuteCommandWithRealtimeOutputAsync(execCommand, resultLine =>
        {
            resultLine = resultLine.TrimEnd('\n');
            if (!string.IsNullOrWhiteSpace(resultLine)) resultList.Add(resultLine);
        });
        if (resultList.Count > 0 && Regex.IsMatch(resultList.Last().ToLower(), @"^conda\s+(\d+(\.\d+)*?)$"))
        {
            CondaEnvExists = true;
        }
    }

    private bool checkCondaInstalled()
    {
        string packagePath = Path.Combine(GetPackageRootPath(), "conda", "Scripts", "conda.exe");
        return File.Exists(packagePath);
    }


    /// <summary>
    /// 釋放資源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_cmdProcess != null)
                {
                    try
                    {
                        // 嘗試正常退出 cmd
                        SendInterrupt();
                        Task.Delay(1000);
                        SendInterrupt();
                        _cmdProcess.StandardInput.WriteLine("exit");
                        _cmdProcess.StandardInput.Flush();

                        // 給一個超時時間讓進程正常退出
                        if (!_cmdProcess.WaitForExit(3000))
                        {
                            _cmdProcess.Kill();
                        }
                    }
                    catch
                    {
                        // 如果無法正常退出，則強制關閉
                        if (!_cmdProcess.HasExited)
                        {
                            _cmdProcess.Kill();
                        }
                    }
                    _cmdProcess.Dispose();
                    _cmdProcess = null;
                }
            }

            _disposed = true;
        }
    }

    ~CommandLineExecutor()
    {
        Dispose(false);
    }
}

public enum ControlKeys
{
    CtrlC,
    CtrlD,
    CtrlZ
}

public class MessageRecord
{
    public DateTime time { get; set; } = DateTime.MinValue;
    public string message { get; set; } = string.Empty;
}