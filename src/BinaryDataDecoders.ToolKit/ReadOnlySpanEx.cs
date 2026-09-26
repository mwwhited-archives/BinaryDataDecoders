using System;
using System.Runtime.InteropServices;

namespace BinaryDataDecoders.ToolKit;

/// <summary>
/// Set of extension methods for ReadOnlySpan&lt;&gt;
/// </summary>
public static partial class ReadOnlySpanEx
{
    /// <summary>
    /// simple extension to allow using ReadOnlySpan&lt;&gt;.StartsWith(...) with an array
    /// </summary>
    /// <param name="data"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static bool StartsWith(this ReadOnlySpan<byte> data, params byte[] pattern) => data.StartsWith(new ReadOnlySpan<byte>(pattern));

    /// <summary>
    ///  transform provided ReadOnlySpan<typeparamref name="TIn"/> into a new Span<typeparamref name="TOut"/>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="input"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Span<TOut> CopyWithTransform<TIn, TOut>(this ReadOnlySpan<TIn> input, Func<TIn, TOut> transform)
        where TOut : struct
    {
        var size = Marshal.SizeOf<TOut>();
        var data = new byte[size * input.Length];
        Span<TOut> target = MemoryMarshal.Cast<byte, TOut>(data.AsSpan());
        CopyToWithTransform(input, target, transform);
        return target;
    }

    /// <summary>
    /// transform provided ReadOnlySpan<typeparamref name="TIn"/> into Existing Span<typeparamref name="TOut"/>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="input"></param>
    /// <param name="target"></param>
    /// <param name="transform"></param>
    public static void CopyToWithTransform<TIn, TOut>(this ReadOnlySpan<TIn> input, Span<TOut> target, Func<TIn, TOut> transform)
    {
        int index = input.Length % 8;
        switch (index)
        {
            case 7: target[6] = transform(input[6]); goto case 6;
            case 6: target[5] = transform(input[5]); goto case 5;
            case 5: target[4] = transform(input[4]); goto case 4;
            case 4: target[3] = transform(input[3]); goto case 3;
            case 3: target[2] = transform(input[2]); goto case 2;
            case 2: target[1] = transform(input[1]); goto case 1;
            case 1: target[0] = transform(input[0]); break;
        }
        for (; index < input.Length; index += 8)
        {
            target[index + 0] = transform(input[index + 0]);
            target[index + 1] = transform(input[index + 1]);
            target[index + 2] = transform(input[index + 2]);
            target[index + 3] = transform(input[index + 3]);
            target[index + 4] = transform(input[index + 4]);
            target[index + 5] = transform(input[index + 5]);
            target[index + 6] = transform(input[index + 6]);
            target[index + 7] = transform(input[index + 7]);
        }
    }
}
