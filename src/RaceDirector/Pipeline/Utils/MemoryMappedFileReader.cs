﻿using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RaceDirector.Pipeline.Utils;

/// <summary>
/// Utility class to reads a struct from a memory mapped file.
/// </summary>
[SupportedOSPlatform("windows")]
public class MemoryMappedFileReader<T> : IDisposable where T : struct
{
    private const long SharedMemoryBegin = 0;
    private const long SharedMemoryEnd = 0;

    private readonly string _path;

    private MemoryMappedFile? _mmFile;

    public MemoryMappedFileReader(string path)
    {
        _path = path;
    }

    /// <summary>
    /// Reads a struct from the memory mapped file at the provided path.
    /// Tries to open it on first access until it succeeds.
    /// </summary>
    /// <exception cref="FileNotFoundException">When opening the file. See <see cref="MemoryMappedFile.OpenExisting"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">When accessing the opened file. See <see cref="MemoryMappedFile.CreateViewStream"/>.</exception>
    /// <exception cref="IOException">When reading from the shared memory. See <see cref="BinaryReader.ReadBytes"/>.</exception>
    /// <remarks>
    /// Not thread safe
    /// </remarks>
    public T Read()
    {
        if (_mmFile == null)
            _mmFile = MemoryMappedFile.OpenExisting(_path, MemoryMappedFileRights.Read);

        using (var viewStream = _mmFile.CreateViewStream(SharedMemoryBegin, SharedMemoryEnd, MemoryMappedFileAccess.Read))
        {
            var safeHandle = viewStream.SafeMemoryMappedViewHandle;
            return Marshal.PtrToStructure<T>(safeHandle.DangerousGetHandle());
        }
    }

    public void Dispose()
    {
        _mmFile?.Dispose();
    }
}