using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using IziHardGames.STUN.Domain.Headers;
using IziHardGames.STUN.STUN;
using static IziHardGames.STUN.Domain.Headers.StunHeader;

namespace IziHardGames.STUN
{
    public static class ExtensionsForLoggerForStun
    {
        public static void LogStunReadHeaderCompletes(this Logger logger, string handlerName)
        {
            var client = logger.target as StunClient;
            var msg = $"LogHandler. Client:{client.name}.   ReactingOnData. Handler:{handlerName}";
            msg = logger.LogSave(msg);

            logger.WriteLine(msg);
        }
        public static void LogHandler(this Logger logger, string handlerName)
        {
            var client = logger.target as StunClient;
            var msg = $"LogHandler. Client:{client.name}.   ReactingOnData. Handler:{handlerName}";
            msg = logger.LogSave(msg);

            logger.WriteLine(msg);
        }
        public static void LogStunHeader(this Logger logger, DataStunHeader dataStunHeader)
        {
            var client = logger.target as StunClient;
            var msg = $"LogStunHeader. Client:{client.name}. Recieved Stun Header: {dataStunHeader.ToStringInfo()}";
            msg = logger.LogSave(msg);

            logger.WriteLine(msg);
        }
        public static void LogAttribute(this Logger logger, AttributeForStun stunAttribute)
        {
            var client = logger.target as StunClient;
            var msg = $"LogAttribute. Client:{client.name}.   Atr:    {stunAttribute.ToStringInfo()}";
            msg = logger.LogSave(msg);

            logger.WriteLine(msg);
        }
        public static void LogAttributeData(this Logger logger, AttributeForStun stunAttribute, ReadOnlySpan<byte> raw, string info = default)
        {
            var client = logger.target as StunClient;
            var msg = $"LogData. Client:{client.name}.   Atr:    {stunAttribute.ToStringInfo()}  AtrData: {raw.ToBase64()}  info:{info}";
            msg = logger.LogSave(msg);

            logger.WriteLine(msg);
        }
        public static void LogAttributeInterpetation(this Logger logger, string info, ConsoleColor color = (ConsoleColor)16)
        {
            StackTrace st = new StackTrace(0, true);
            StackFrame[] stFrames = st.GetFrames();

            var frame = stFrames[1];
            var methodBase = frame.GetMethod();

            var client = logger.target as StunClient;
            var msg = $"Interpetation. Client:{client.name}.    [{info}]   {methodBase.DeclaringType}.{methodBase.Name}(). line:{frame.GetFileLineNumber()}. col:{frame.GetFileColumnNumber()}";
            msg = logger.LogSave(msg);

            logger.WriteLine(msg, color);
        }

        public static void LogAttrError(this Logger logger, StunErrorCode stunErrorCode, ReadOnlySpan<byte> memory)
        {
            var errorMessage = Encoding.UTF8.GetString(memory);
#if DEBUG
            ConsoleWrap.Red($"{BitConverter.ToString(BitConverter.GetBytes(stunErrorCode.comboClassAndNumber))} {stunErrorCode.GetCode()}: {errorMessage}");
#endif
        }
    }
}
