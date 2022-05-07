using RaceDirector.Pipeline.Utils;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Utils
{
    [IntegrationTest]
    [SupportedOSPlatform("windows")]
    public class MemoryMappedFileReaderTest
    {
        const string FileName = "$rdtest";

        [Fact]
        public void ThrowsExceptionOnReadWhenPathDoesntExist()
        {
            using var reader = new MemoryMappedFileReader<TestStruct>(FileName);
            Assert.Throws<FileNotFoundException>(() => reader.Read());

            WithMMFile(FileName, new TestStruct(), (_, _) => {
                // Verify that file is opened when available
                reader.Read();
            });
        }

        [Fact]
        public void CanReadAMemoryMappedStruct()
        {
            var expectedStruct = new TestStruct()
            {
                Field = SequentialByteArray(FieldSize)
            };
            
            WithMMFile(FileName, expectedStruct, (path, writtenData) => {
                using (var reader = new MemoryMappedFileReader<TestStruct>(path))
                {
                    Assert.Equal(writtenData.Field, reader.Read().Field);
                    // Verify that the same data can be read more than once
                    Assert.Equal(writtenData.Field, reader.Read().Field);
                }
            });
        }

        private void WithMMFile<T>(string path, T data, Action<string, T> action) where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));
            Assert.Equal(FieldSize, structSize);
            using var file = MemoryMappedFile.CreateNew(path, FieldSize);
            using var viewStream = file.CreateViewStream();
            var writer = new BinaryWriter(viewStream);
            writer.Write(Serialize(data));
            action(path, data);
        }

        private byte[] SequentialByteArray(int size)
        {
            return Enumerable.Range(1, size).Select(x => Convert.ToByte(x)).ToArray();
        }

        private static byte[] Serialize<T>(T s) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(s, ptr, true);
            Marshal.Copy(ptr, array, 0, size);
            Marshal.FreeHGlobal(ptr);
            return array;
        }

        const int FieldSize = 53;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TestStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = FieldSize)]
            public byte[] Field;
        }
    }
}
