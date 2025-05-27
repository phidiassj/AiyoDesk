using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiyoLibraryV2.AiService.Extensions
{
    public class StaticFunctions
    {
        public static bool IsProductionEnviroment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null) return false;
            return (environment.ToLower().Contains("production"));
        }

        public static string? GetTagContent(string SourceText, string TagName)
        {
            string startTag = "<" + TagName + ">";
            string endTag = "</" + TagName + ">";

            int startIndex = SourceText.IndexOf(startTag) + startTag.Length;
            int endIndex = SourceText.IndexOf(endTag, startIndex);

            if (startIndex < 0 || endIndex < 0)
            {
                return null; // Tag not found
            }

            return SourceText.Substring(startIndex, endIndex - startIndex).Trim('\n').TrimStart();
        }

        public static string? GetTagContentIncudeTag(string SourceText, string TagName)
        {
            string startTag = "<" + TagName + ">";
            string endTag = "</" + TagName + ">";

            int startIndex = SourceText.IndexOf(startTag);
            int endIndex = SourceText.IndexOf(endTag) + endTag.Length;

            if (startIndex < 0 || endIndex < 0)
            {
                return null; // Tag not found
            }

            return SourceText.Substring(startIndex, endIndex - startIndex).Trim('\n').TrimStart();
        }

        public static List<string> SplitText(string text, int words)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < text.Length; i += words)
            {
                if (i + words > text.Length) words = text.Length - i;
                list.Add(text.Substring(i, words));
            }
            return list;
        }

        public static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length || vector1.Length == 0)
            {
                throw new ArgumentException("輸入向量的維度不正確或為空");
            }

            // 計算兩向量的內積
            double dotProduct = vector1.Zip(vector2, (x, y) => (double)x * y).Sum();

            // 計算每個向量的長度
            double length1 = Math.Sqrt(vector1.Sum(x => x * x));
            double length2 = Math.Sqrt(vector2.Sum(y => y * y));

            // 計算 Cosine Similarity
            if (length1 == 0 || length2 == 0)
            {
                return 0.0; // 避免除以零
            }
            else
            {
                return dotProduct / (length1 * length2);
            }
        }

        public static double CalculateCosineSimilarity(byte[] vector1Bytes, byte[] vector2Bytes)
        {
            // 將 byte[] 轉換回浮點數陣列
            float[] vector1 = ConvertByteAryToFloatAry(vector1Bytes);
            float[] vector2 = ConvertByteAryToFloatAry(vector2Bytes);

            return CalculateCosineSimilarity(vector1, vector2);
        }

        public static float[] ConvertByteAryToFloatAry(byte[] source)
        {
            if (source == null || source.Length <= 3) throw new ArgumentException("source is null");
            float[] result = new float[source.Length / 4];
            for (int iCnt = 0; iCnt < source.Length; iCnt += 4)
            {
                result[iCnt / 4] = BitConverter.ToSingle(source, iCnt);
            }
            return result;
        }

        public static byte[]? ConvertFloatAryToByteAry(float[] source)
        {
            if (source == null || source.Length <= 0) return null;
            byte[] result = new byte[source.Length * 4];
            for (int iCnt = 0; iCnt < source.Length; iCnt++)
            {
                byte[] bytes = BitConverter.GetBytes(source[iCnt]);
                Array.Copy(bytes, 0, result, iCnt * 4, 4);
            }
            return result;
        }

        public static DateTime GetGmt8DateTime()
        {
            DateTime now = DateTime.Now;

            // 轉換為 UTC 時間
            DateTime utcNow = now.ToUniversalTime();

            // 創建 GMT+8 時區
            TimeZoneInfo gmt8 = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

            // 將 UTC 時間轉換為 GMT+8 時間
            DateTime gmt8Now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, gmt8);

            return gmt8Now;
        }

        public static DateTime ConvertNYTimeToGmt8Time(DateTime dt)
        {
            TimeZoneInfo newYorkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            TimeZoneInfo taiwanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dt, newYorkTimeZone);
            DateTime taiwanTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, taiwanTimeZone);

            return taiwanTime;
        }

        public static AIPlatform ConvertAIPlatformNameToValue(string AIPlatformName)
        {
            if (AIPlatformName.ToLower() == "google") return AIPlatform.Google;
            if (AIPlatformName.ToLower() == "anthropic") return AIPlatform.Anthropic;
            if (AIPlatformName.ToLower() == "openai") return AIPlatform.Anthropic;
            throw new ArgumentException("Unrecognize platform name.");
        }

    }

    public enum AIPlatform
    {
        OpenAI,
        Anthropic,
        Google
    }

    public enum AIModelEncodeType
    {
        /// <summary>
        /// GPT-3 models like davinci
        /// </summary>
        r50k_base,
        /// <summary>
        /// Codex models, text-davinci-002, text-davinci-003
        /// </summary>
        p50k_base,
        p50k_edit,
        /// <summary>
        /// gpt-4, gpt-3.5-turbo, text-embedding-ada-002
        /// </summary>
        cl100k_base
    }

    public enum ModelUseFor
    {
        Chat,
        Embedding,
        Speech
    }

    public enum ChatStopReasons
    {
        stop,               // if the model hit a natural stop point or a provided stop sequence
        stream,             // still in streaming
        length,             // if the maximum number of tokens specified in the request was reached
        content_filter,     // if content was omitted due to a flag from our content filters
        tool_calls,         // if the model called a tool
        function_call,      // (deprecated) if the model called a function
        error
    }

    public enum ChatContentType
    {
        text,
        binary,
        audio
    }

    public enum ChatRoleType
    {
        system,
        model,
        user,
        tool
    }

}
