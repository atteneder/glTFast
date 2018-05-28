/// Use System.Buffer.MemoryCopy (instead of Marshal.Copy)
/// turned out to be slower in most cases.
//#define BUFFER_MEMORY_COPY

/// TODO: make extensive test and compare different copy/conversion methods on target platforms:
/// System.Buffer.MemoryCopy
/// System.Buffer.BlockCopy
/// Marshal.Copy
/// native (c/c++) solutions

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

namespace GLTFast {

	using Schema;

	public static class Extractor {
        
		const float UINT16_MAX = 65535f;

		public unsafe static byte[] CreateBufferViewCopy(BufferView bufferView, GlbBinChunk chunk, byte[] bytes)
        {
            var result = new byte[bufferView.byteLength];
            fixed (void* p = &(result[0]), src = &(bytes[bufferView.byteOffset + chunk.start]))
            {
                System.Buffer.MemoryCopy(
                    src,
                    p,
                    bufferView.byteLength,
                    bufferView.byteLength
                );
            }
            return result;
        }

		public unsafe static int[] GetIndicesUInt8(byte[] bytes, int start, int count)
        {
            var res = new int[count];
            for (var i = 0; i < count; i++)
            {
                res[i] = bytes[start + i];
            }
            return res;
        }

		public unsafe static int[] GetIndicesUInt16(byte[] bytes, int start, int count)
        {
            var res = new int[count];
            for (var i = 0; i < count; i++)
            {
                res[i] = BitConverter.ToUInt16(bytes, start + i * 2);
            }
            return res;
        }

		public unsafe static int[] GetIndicesUInt32(byte[] bytes, int start, int count)
        {
            var res = new int[count];
            System.Buffer.BlockCopy(bytes, start, res, 0, count * 4);
            return res;
        }

		public unsafe static Vector3[] GetVector3s(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetVector3s");
            var res = new Vector3[count];

#if BUFFER_MEMORY_COPY_MEMORY_COPY
            fixed( void* p = &(res[0]), src = &(bytes[start]) ) {
                System.Buffer.MemoryCopy(
                    src,
                    p,
                    count*12,
                    count*12
                );
            }
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);
            Marshal.Copy(bytes, start, gcRes.AddrOfPinnedObject(), count * 12);
            gcRes.Free();
#endif

            Profiler.EndSample();
            return res;
        }
  
        // The generic version turned out to be a bit slower than concrete implementations
        // TODO: maybe re-evaluate
        /*
		public static VectorFormat[] GetVectors<VectorFormat>(
            ref byte[] bytes,
            int start,
            int count
            ) where VectorFormat : struct
        {
            Profiler.BeginSample("GetVectors");
            var res = new VectorFormat[count];
            int byteStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VectorFormat));

            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);
            Marshal.Copy(bytes, start, gcRes.AddrOfPinnedObject(), count * byteStride);
            gcRes.Free();

            Profiler.EndSample();
            return res;
        }

		public static VectorFormat[] GetVectorsInterleaved<VectorFormat>(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
            ) where VectorFormat : struct
        {
            Profiler.BeginSample("GetVectorsInterleaved");
            var res = new VectorFormat[count];
            int elementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VectorFormat));

            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);

            int off = start;
            var dest = gcRes.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.Copy(bytes, off, dest, elementSize);
                off += byteStride;
                dest += elementSize;
            }

            gcRes.Free();

            Profiler.EndSample();
            return res;
        }
        */

		public unsafe static Vector3[] GetVector3sInterleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetVector3sInterleaved");
            var res = new Vector3[count];

#if BUFFER_MEMORY_COPY
            fixed (Vector3*
                   dest = &(res[0])
                  )
            {
                Vector3* destV = dest;
                fixed (byte* src = &(bytes[start]))
                {
                    byte* off = src;
                    for (int i = 0; i < count; i++)
                    {
                        System.Buffer.MemoryCopy(
                            off,
                            destV,
                            12,
                            12
                        );
                        off += byteStride;
                        destV += 1;
                    }
                }
            }
#else
            int elementSize = Marshal.SizeOf(typeof(Vector3));

            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);

            int off = start;
            var dest = gcRes.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.Copy(bytes, off, dest, elementSize);
                off += byteStride;
                dest += elementSize;
            }

            gcRes.Free();
#endif
            Profiler.EndSample();
            return res;
        }

		public unsafe static Vector4[] GetVector4s(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetVector4s");
            var res = new Vector4[count];

#if BUFFER_MEMORY_COPY
            fixed( void* p = &(res[0]), src = &(bytes[start]) ) {
                System.Buffer.MemoryCopy(
                    src,
                    p,
                    count*16,
                    count*16
                );
            }
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);
            Marshal.Copy(bytes, start, gcRes.AddrOfPinnedObject(), count * 16);
            gcRes.Free();
#endif

            Profiler.EndSample();
            return res;
        }

		public unsafe static Vector4[] GetVector4sInterleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetVector4sInterleaved");
            var res = new Vector4[count];

#if BUFFER_MEMORY_COPY
            fixed (Vector4*
                   dest = &(res[0])
                  )
            {
                Vector4* destV = dest;
                fixed (byte* src = &(bytes[start]))
                {
                    byte* off = src;
                    for (int i = 0; i < count; i++)
                    {
                        System.Buffer.MemoryCopy(
                            off,
                            destV,
                            16,
                            16
                        );
                        off += byteStride;
                        destV += 1;
                    }
                }
            }
#else
            int elementSize = Marshal.SizeOf(typeof(Vector4));

            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);

            int off = start;
            var dest = gcRes.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.Copy(bytes, off, dest, elementSize);
                off += byteStride;
                dest += elementSize;
            }

            gcRes.Free();
#endif
            Profiler.EndSample();
            return res;
        }

		public static Color[] GetColorsVec3Float(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetColorsVec3Float");
            var res = new Color[count];

            // TODO: try partial memcopy and compare performance.
            for (var i = 0; i < count; i++)
            {
                res[i].r = BitConverter.ToSingle(bytes, start + i * 12);
                res[i].g = BitConverter.ToSingle(bytes, start + i * 12 + 4);
                res[i].b = BitConverter.ToSingle(bytes, start + i * 12 + 8);
                res[i].a = 1.0f;
            }

            Profiler.EndSample();
            return res;
        }

        public static Color[] GetColorsVec3FloatInterleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetColorsVec3FloatInterleaved");
            var res = new Color[count];

            // TODO: try partial memcopy and compare performance.
            for (var i = 0; i < count; i++)
            {
                res[i].r = BitConverter.ToSingle(bytes, start + i * byteStride);
                res[i].g = BitConverter.ToSingle(bytes, start + i * byteStride + 4);
                res[i].b = BitConverter.ToSingle(bytes, start + i * byteStride + 8);
                res[i].a = 1.0f;
            }

            Profiler.EndSample();
            return res;
        }

		public unsafe static Color[] GetColorsVec4Float(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            // TODO: maybe do generic with GetVector4s?

            Profiler.BeginSample("GetColorsVec4Float");
            var res = new Color[count];

#if BUFFER_MEMORY_COPY
            fixed( void* p = &(res[0]), src = &(bytes[start]) ) {
                System.Buffer.MemoryCopy(
                    src,
                    p,
                    count*16,
                    count*16
                );
            }
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);
            Marshal.Copy(bytes, start, gcRes.AddrOfPinnedObject(), count * 16);
            gcRes.Free();
#endif

            Profiler.EndSample();
            return res;
        }

        public unsafe static Color[] GetColorsVec4FloatInterleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
			// TODO: maybe do generic with GetVector4sInterleaved ?

            Profiler.BeginSample("GetColorsVec4FloatInterleaved");
            var res = new Color[count];
#if BUFFER_MEMORY_COPY
            fixed (Color*
                   dest = &(res[0])
                  )
            {
                Color* destV = dest;
                fixed (byte* src = &(bytes[start]))
                {
                    byte* off = src;
                    for (int i = 0; i < count; i++)
                    {
                        System.Buffer.MemoryCopy(
                            off,
                            destV,
                            16,
                            16
                        );
                        off += byteStride;
                        destV += 1;
                    }
                }
            }
#else
            int elementSize = Marshal.SizeOf(typeof(Color));

            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);

            int off = start;
            var dest = gcRes.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.Copy(bytes, off, dest, elementSize);
                off += byteStride;
                dest += elementSize;
            }

            gcRes.Free();
#endif

            Profiler.EndSample();
            return res;
        }

		public static Color32[] GetColorsVec3UInt8(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetColorsVec3UInt8");
            var res = new Color32[count];

            for (var i = 0; i < count; i++)
            {
                res[i].r = bytes[start + i * 3];
                res[i].g = bytes[start + i * 3 + 1];
                res[i].b = bytes[start + i * 3 + 2];
                res[i].a = 255;
            }

            Profiler.EndSample();
            return res;
        }

        public static Color32[] GetColorsVec3UInt8Interleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetColorsVec3UInt8Interleaved");
            var res = new Color32[count];

            //Todo: check faster copy method
            for (var i = 0; i < count; i++)
            {
                res[i].r = bytes[start + i * byteStride];
                res[i].g = bytes[start + i * byteStride + 1];
                res[i].b = bytes[start + i * byteStride + 2];
                res[i].a = 255;
            }

            Profiler.EndSample();
            return res;
        }

		public unsafe static Color32[] GetColorsVec4UInt8(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetColorsVec4UInt8");
            var res = new Color32[count];

#if BUFFER_MEMORY_COPY
            fixed( void* p = &(res[0]), src = &(bytes[start]) ) {
                System.Buffer.MemoryCopy(
                    src,
                    p,
                    count*4,
                    count*4
                );
            }
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);
            Marshal.Copy(bytes, start, gcRes.AddrOfPinnedObject(), count * 4);
            gcRes.Free();
#endif
            Profiler.EndSample();
            return res;
        }

        public unsafe static Color32[] GetColorsVec4UInt8Interleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetColorsVec4UInt8Interleaved");
            var res = new Color32[count];

            int elementSize = Marshal.SizeOf<Color32>();
#if BUFFER_MEMORY_COPY
            fixed (Color32* dest = &(res[0]))
            {
                Color32* destV = dest;
                fixed (byte* src = &(bytes[start]))
                {
                    byte* off = src;
                    for (int i = 0; i < count; i++)
                    {
                        System.Buffer.MemoryCopy(
                            off,
                            destV,
                            elementSize,
                            elementSize
                        );
                        off += byteStride;
                        destV += 1;
                    }
                }
            }
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);

            int off = start;
            var dest = gcRes.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.Copy(bytes, off, dest, elementSize);
                off += byteStride;
                dest += elementSize;
            }

            gcRes.Free();
#endif
            Profiler.EndSample();
            return res;
        }

		public static Color[] GetColorsVec3UInt16(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetColorsVec3UInt16");
            var res = new Color[count];

            for (var i = 0; i < count; i++)
            {
                res[i].r = BitConverter.ToUInt16(bytes, start + i * 6) / UINT16_MAX;
                res[i].g = BitConverter.ToUInt16(bytes, start + i * 6 + 2) / UINT16_MAX;
                res[i].b = BitConverter.ToUInt16(bytes, start + i * 6 + 4) / UINT16_MAX;
                res[i].a = 1;
            }

            Profiler.EndSample();
            return res;
        }

        public static Color[] GetColorsVec3UInt16Interleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetColorsVec3UInt16Interleaved");
            var res = new Color[count];

            for (var i = 0; i < count; i++)
            {
                res[i].r = BitConverter.ToUInt16(bytes, start + byteStride * 6) / UINT16_MAX;
                res[i].g = BitConverter.ToUInt16(bytes, start + byteStride * 6 + 2) / UINT16_MAX;
                res[i].b = BitConverter.ToUInt16(bytes, start + byteStride * 6 + 4) / UINT16_MAX;
                res[i].a = 1;
            }

            Profiler.EndSample();
            return res;
        }

		public static Color[] GetColorsVec4UInt16(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetColorsVec4UInt16");
            var res = new Color[count];

            for (var i = 0; i < count; i++)
            {
                res[i].r = BitConverter.ToUInt16(bytes, start + i * 8) / UINT16_MAX;
                res[i].g = BitConverter.ToUInt16(bytes, start + i * 8 + 2) / UINT16_MAX;
                res[i].b = BitConverter.ToUInt16(bytes, start + i * 8 + 4) / UINT16_MAX;
                res[i].a = BitConverter.ToUInt16(bytes, start + i * 8 + 6) / UINT16_MAX;
            }

            Profiler.EndSample();
            return res;
        }

        public static Color[] GetColorsVec4UInt16Interleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetColorsVec4UInt16Interleaved");
            var res = new Color[count];

            // TODO: test if memcpy method is faster
            for (var i = 0; i < count; i++)
            {
                res[i].r = BitConverter.ToUInt16(bytes, start + byteStride * i) / UINT16_MAX;
                res[i].g = BitConverter.ToUInt16(bytes, start + byteStride * i + 2) / UINT16_MAX;
                res[i].b = BitConverter.ToUInt16(bytes, start + byteStride * i + 4) / UINT16_MAX;
                res[i].a = BitConverter.ToUInt16(bytes, start + byteStride * i + 6) / UINT16_MAX;
            }

            Profiler.EndSample();
            return res;
        }

		public unsafe static Vector2[] GetUVsFloat(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetUVsFloat");
            var res = new Vector2[count];

#if BUFFER_MEMORY_COPY
            fixed( void* p = &(res[0]), src = &(bytes[start]) ) {
                System.Buffer.MemoryCopy(
                    src,
                    p,
                    count*8,
                    count*8
                );
            }
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);
            Marshal.Copy(bytes, start, gcRes.AddrOfPinnedObject(), count * 8);
            gcRes.Free();
#endif
            for (var i = 0; i < count; i++)
            {
                res[i].y = 1 - res[i].y;
            }

            Profiler.EndSample();
            return res;
        }

        public unsafe static Vector2[] GetUVsFloatInterleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetUVsFloatInterleaved");
            var res = new Vector2[count];

            int elementSize = Marshal.SizeOf(typeof(Vector2));
#if BUFFER_MEMORY_COPY
            fixed (Vector2* dest = &(res[0]))
			{
				Vector2* destV = dest;
				fixed (byte* src = &(bytes[start]))
				{
					byte* off = src;
					for (int i = 0; i < count; i++)
					{
						System.Buffer.MemoryCopy(
							off,
							destV,
							elementSize,
							elementSize
						);
						off += byteStride;
						destV += 1;
					}
				}
			}
#else
            var gcRes = GCHandle.Alloc(res, GCHandleType.Pinned);

            int off = start;
            var dest = gcRes.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.Copy(bytes, off, dest, elementSize);
                off += byteStride;
                dest += elementSize;
            }

            gcRes.Free();
#endif
            for (var i = 0; i < count; i++)
            {
                res[i].y = 1 - res[i].y;
            }

            Profiler.EndSample();
            return res;
        }

		public unsafe static Vector2[] GetUVsUInt8(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetUVsUInt8");
            var res = new Vector2[count];

            for (var i = 0; i < count; i++)
            {
                res[i].x = bytes[start + (i * 2)] / 255f;
                res[i].y = 1 - bytes[start + (i * 2) + 1] / 255f;
            }

            Profiler.EndSample();
            return res;
        }

        public unsafe static Vector2[] GetUVsUInt8Interleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetUVsUInt8Interleaved");
            var res = new Vector2[count];

            int index = start;
            for (var i = 0; i < count; i++)
            {
                res[i].x = bytes[index] / 255f;
                res[i].y = 1 - bytes[index+1] / 255f;
                index += byteStride;
            }

            Profiler.EndSample();
            return res;
        }

		public unsafe static Vector2[] GetUVsUInt16(
            ref byte[] bytes,
            int start,
            int count
        )
        {
            Profiler.BeginSample("GetUVsUInt16");
            var res = new Vector2[count];

            for (var i = 0; i < count; i++)
            {
                res[i].x = BitConverter.ToUInt16(bytes, start + i * 4) / UINT16_MAX;
                res[i].y = 1 - BitConverter.ToUInt16(bytes, start + i * 4 + 2) / UINT16_MAX;
            }

            Profiler.EndSample();
            return res;
        }

        public unsafe static Vector2[] GetUVsUInt16Interleaved(
            ref byte[] bytes,
            int start,
            int count,
            int byteStride
        )
        {
            Profiler.BeginSample("GetUVsUInt16Interleaved");
            var res = new Vector2[count];

            int index = start;
            for (var i = 0; i < count; i++)
            {
                res[i].x = BitConverter.ToUInt16(bytes, index) / UINT16_MAX;
                res[i].y = 1 - BitConverter.ToUInt16(bytes, index + 2) / UINT16_MAX;
                index += byteStride;
            }

            Profiler.EndSample();
            return res;
        }
	}
}
