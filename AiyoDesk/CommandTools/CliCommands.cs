
namespace AiyoDesk.CommanandTools;

public class CliCommands
{
    public static string ActivateConda = @"C:\ProgramData\miniforge3\Scripts\activate.bat C:\ProgramData\miniforge3";
    public static string ActivateLlmEnv = @"conda activate llm";
    public static string ActivateLlamaCpp = @"D:\AI\llama-b4988-bin-win-sycl-x64\llama-server -m D:\AI\gemma-3-it-GGUF\gemma-3-4b-it-Q8_0.gguf --alias gemma3 --port 16888 -t 2 -c 40960 -ngl 50 -sm layer --temp 0.2 --jinja -fa --path D:\AI\llama-cpp-ipex-llm-2.2.0b20250313-win\public";
    //public static string ActivateLlamaCpp = @"D:\AI\llama-cpp-ipex-llm-2.2.0b20250313-win\llama-server -m D:\AI\gemma-3-it-GGUF\gemma-3-4b-it-Q8_0.gguf --alias gemma3 --port 16888 -t 2 -c 40960 -ngl 50 -sm layer --temp 0.2 --jinja -fa --path D:\AI\llama-cpp-ipex-llm-2.2.0b20250313-win\public";

    public static int LlamaCppPort = 16888;
}
