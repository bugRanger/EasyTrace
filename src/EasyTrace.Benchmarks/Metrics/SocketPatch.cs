using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyTrace.Benchmarks.Metrics;

public class SocketPatch
{
    private static readonly SocketDiagnoser SocketDiagnoser = new();

    public static SocketDiagnoser Initialize()
    {
        var harmony = new Harmony("com.easytrace.socket");

        var originalMethod = typeof(Socket).GetMethod(
            nameof(Socket.Send),
            BindingFlags.Public | BindingFlags.Instance,
            null,
            [typeof(ReadOnlySpan<byte>), typeof(SocketFlags), typeof(SocketError).MakeByRefType()],
            null
        );

        if (originalMethod == null)
        {
            throw new Exception($"Not found {nameof(Socket.Send)}");
        }

        var transpilerMethod = typeof(SocketPatch).GetMethod(
            nameof(Transpiler),
            BindingFlags.Public | BindingFlags.Static
        );

        var harmonyTranspiler = new HarmonyMethod(transpilerMethod);
        harmony.Patch(originalMethod, transpiler: harmonyTranspiler);

        return SocketDiagnoser;
    }

    public static void InterceptAfterSend(Socket socket, ReadOnlySpan<byte> buffer, SocketError errorCode, int sent)
    {
        if (errorCode == SocketError.Success)
        {
            SocketDiagnoser.IncrementSend();
        }

#if DEBUG
        if (errorCode == SocketError.Success)
        {
            byte[] sentBytes = buffer.ToArray();
            string text = Encoding.UTF8.GetString(sentBytes);

            Console.WriteLine($"[Socket.Send] Sent {bytesSent} bytes.");
            Console.WriteLine($"-> Remote: {socket.RemoteEndPoint}");
            Console.WriteLine($"-> Payload: {text}");
        }
        else
        {
            Console.WriteLine($"[Socket.Send] Error: {errorCode}");
        }
#endif
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var interceptMethod = typeof(SocketPatch).GetMethod(
            nameof(InterceptAfterSend),
            BindingFlags.Public | BindingFlags.Static
        );

        var list = new List<CodeInstruction>(instructions);

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].opcode != OpCodes.Ret)
            {
                continue;
            }

            var injection = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0), // arg 0: this (Socket)
                new CodeInstruction(OpCodes.Ldarg_1), // arg 1: ReadOnlySpan<byte> buffer
                new CodeInstruction(OpCodes.Ldarg_3), // arg 3: out SocketError errorCode
                new CodeInstruction(OpCodes.Ldobj, typeof(SocketError)),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, interceptMethod)
            };

            list.InsertRange(i, injection);
            i += injection.Count;
        }

        return list;
    }
}