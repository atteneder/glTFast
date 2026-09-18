// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;

namespace GLTFast
{
    /// <summary>
    /// Reads files directly into native memory.
    /// </summary>
    static class NativeFileReader
    {
        /// <summary>
        /// Reads a file's content into a <see cref="NativeArray{T}"/>, without allocating managed memory.
        /// </summary>
        /// <param name="path">Path of the file to read.</param>
        /// <param name="data">Content of the file. Only valid if the method returned true. Callers own it and are
        /// required to dispose it.</param>
        /// <param name="error">Description of what went wrong. Null if the method returned true.</param>
        /// <returns>True if the file was read entirely, false otherwise.</returns>
        public static unsafe bool TryReadAllBytes(string path, out NativeArray<byte> data, out string error)
        {
            data = default;

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                error = $"Cannot find resource at path {path}";
                return false;
            }

            var size = fileInfo.Length;
            if (size > int.MaxValue)
            {
                error = $"File at path {path} exceeds the 2GB limit.";
                return false;
            }

            data = new NativeArray<byte>((int)size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            if (size == 0)
            {
                error = null;
                return true;
            }

            var readCommand = new ReadCommand
            {
                Offset = 0,
                Size = size,
                Buffer = data.GetUnsafePtr()
            };

            var readHandle = AsyncReadManager.Read(path, &readCommand, 1);
            readHandle.JobHandle.Complete();
            var status = readHandle.Status;
            readHandle.Dispose();

            if (status == ReadStatus.Complete)
            {
                error = null;
                return true;
            }

            data.Dispose();
            data = default;
            error = $"Reading file at path {path} failed ({status}).";
            return false;
        }
    }
}
