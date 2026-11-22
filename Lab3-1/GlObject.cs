using Silk.NET.OpenGL;
using System;

namespace Lab3
{
    internal class GlObject
    {
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }

        private GL Gl;

        public GlObject(uint vao, uint vertices, uint colors, uint indices, uint indexArrayLength, GL gl)
        {
            this.Vao = vao;
            this.Vertices = vertices;
            this.Colors = colors;
            this.Indices = indices;
            this.IndexArrayLength = indexArrayLength;
            this.Gl = gl;
        }

        public static unsafe GlObject CreateTubRectangle(GL Gl, bool smoothNormals)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float nxLeft = 0f, nzLeft = 1f;
            float nxRight = 0f, nzRight = 1f;

            if (smoothNormals)
            {
                float angle10 = 10f * MathF.PI / 180f;

                nxLeft = -MathF.Sin(angle10);
                nzLeft = MathF.Cos(angle10);

                nxRight = MathF.Sin(angle10);
                nzRight = MathF.Cos(angle10);
            }

            float[] vertexArray = new float[] {
                -0.5f, 1.0f, 0.0f, nxLeft, 0f, nzLeft,
                -0.5f, -1.0f, 0.0f, nxLeft, 0f, nzLeft,
                0.5f, -1.0f, 0.0f, nxRight, 0f, nzRight,
                0.5f, 1.0f, 0.0f, nxRight, 0f, nzRight
            };

            float[] colorArray = new float[] {
                0.6f, 0.3f, 0.1f, 1.0f,
                0.6f, 0.3f, 0.1f, 1.0f,
                0.6f, 0.3f, 0.1f, 1.0f,
                0.6f, 0.3f, 0.1f, 1.0f
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3
            };

            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint vertexSize = offsetNormals + 3 * sizeof(float);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertices);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, colors);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indices);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), BufferUsageARB.StaticDraw);

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            Gl.BindVertexArray(0);

            return new GlObject(vao, vertices, colors, indices, (uint)indexArray.Length, Gl);
        }

        internal void Release()
        {
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }
    }
}
