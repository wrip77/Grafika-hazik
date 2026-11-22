using Lab3_3;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System;
using System.Numerics;

namespace Szeminarium
{
    internal static class Program
    {
        private static IWindow graphicWindow;
        private static GL Gl;
        private static uint program;
        private static ImGuiController imGuiController;

        private static CameraDescriptor camera = new CameraDescriptor();
        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private static uint[] vaos = new uint[27];
        private static Matrix4X4<float>[] modelMatrixok = new Matrix4X4<float>[27];
        private static bool[] forgoSzeletenVan = new bool[27];

        private static Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);
        private static float lightPosX = 5.0f;
        private static float lightPosY = 5.0f;
        private static float lightPosZ = 5.0f;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec4 vCol;
        layout (location = 2) in vec3 vNormal;

        uniform mat4 uModel;
        uniform mat4 uSliceRotation;
        uniform mat4 uView;
        uniform mat4 uProjection;
        uniform mat3 uNormalMatrix;

        out vec4 outCol;
        out vec3 outNormal;
        out vec3 outWorldPosition;
        
        void main()
        {
            outCol = vCol;
            mat4 worldMatrix = uSliceRotation * uModel;
            outWorldPosition = vec3(worldMatrix * vec4(vPos, 1.0));
            outNormal = uNormalMatrix * vNormal;
            gl_Position = uProjection * uView * worldMatrix * vec4(vPos, 1.0);
        }
        ";

        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;

        in vec4 outCol;
        in vec3 outNormal;
        in vec3 outWorldPosition;

        uniform vec3 lightColor;
        uniform vec3 lightPos;
        uniform vec3 viewPos;

        void main()
        {
            float ambientStrength = 0.4;
            vec3 ambient = ambientStrength * lightColor;

            vec3 norm = normalize(outNormal);
            vec3 lightDir = normalize(lightPos - outWorldPosition);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * lightColor;
            
            float specularStrength = 0.3;
            vec3 viewDir = normalize(viewPos - outWorldPosition);
            vec3 reflectDir = reflect(-lightDir, norm);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
            vec3 specular = specularStrength * spec * lightColor;  
            
            vec3 result = (ambient + diffuse + specular) * outCol.rgb;
            FragColor = vec4(result, outCol.a);
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Rubik Kocka Phong + ImGui";
            windowOptions.Size = new Vector2D<int>(800, 800);
            windowOptions.PreferredDepthBufferBits = 24;

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static unsafe void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            imGuiController = new ImGuiController(Gl, graphicWindow, inputContext);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
            Gl.ClearColor(System.Drawing.Color.LightGray);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);

            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            float[] vertexArray = {
                -0.5f, -0.5f,  0.5f,   0.0f,  0.0f,  1.0f,
                 0.5f, -0.5f,  0.5f,   0.0f,  0.0f,  1.0f,
                 0.5f,  0.5f,  0.5f,   0.0f,  0.0f,  1.0f,
                -0.5f,  0.5f,  0.5f,   0.0f,  0.0f,  1.0f,

                 0.5f, -0.5f, -0.5f,   0.0f,  0.0f, -1.0f,
                -0.5f, -0.5f, -0.5f,   0.0f,  0.0f, -1.0f,
                -0.5f,  0.5f, -0.5f,   0.0f,  0.0f, -1.0f,
                 0.5f,  0.5f, -0.5f,   0.0f,  0.0f, -1.0f,

                -0.5f, -0.5f, -0.5f,  -1.0f,  0.0f,  0.0f,
                -0.5f, -0.5f,  0.5f,  -1.0f,  0.0f,  0.0f,
                -0.5f,  0.5f,  0.5f,  -1.0f,  0.0f,  0.0f,
                -0.5f,  0.5f, -0.5f,  -1.0f,  0.0f,  0.0f,

                 0.5f, -0.5f,  0.5f,   1.0f,  0.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,   1.0f,  0.0f,  0.0f,
                 0.5f,  0.5f, -0.5f,   1.0f,  0.0f,  0.0f,
                 0.5f,  0.5f,  0.5f,   1.0f,  0.0f,  0.0f,

                -0.5f,  0.5f,  0.5f,   0.0f,  1.0f,  0.0f,
                 0.5f,  0.5f,  0.5f,   0.0f,  1.0f,  0.0f,
                 0.5f,  0.5f, -0.5f,   0.0f,  1.0f,  0.0f,
                -0.5f,  0.5f, -0.5f,   0.0f,  1.0f,  0.0f,

                -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,
                 0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,
                 0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f,
                -0.5f, -0.5f,  0.5f,   0.0f, -1.0f,  0.0f
            };

            uint[] indexArray = {
                0, 1, 2,  0, 2, 3,
                4, 5, 6,  4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23
            };

            uint verticesBuffer = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, verticesBuffer);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);

            uint indicesBuffer = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indicesBuffer);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            int kockaIndex = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        vaos[kockaIndex] = Gl.GenVertexArray();
                        Gl.BindVertexArray(vaos[kockaIndex]);

                        Gl.BindBuffer(GLEnum.ArrayBuffer, verticesBuffer);

                        Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), null);
                        Gl.EnableVertexAttribArray(0);

                        Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
                        Gl.EnableVertexAttribArray(2);

                        float[] colorArray = new float[96];

                        for (int i = 0; i < 96; i += 4)
                        {
                            colorArray[i] = 0.2f;
                            colorArray[i + 1] = 0.2f;
                            colorArray[i + 2] = 0.2f;
                            colorArray[i + 3] = 1.0f;
                        }

                        if (z == 1) { for (int i = 0; i < 16; i += 4) { colorArray[i] = 1.0f; colorArray[i + 1] = 0.0f; colorArray[i + 2] = 0.0f; } }
                        if (z == -1) { for (int i = 16; i < 32; i += 4) { colorArray[i] = 0.0f; colorArray[i + 1] = 0.0f; colorArray[i + 2] = 1.0f; } }
                        if (x == -1) { for (int i = 32; i < 48; i += 4) { colorArray[i] = 1.0f; colorArray[i + 1] = 0.5f; colorArray[i + 2] = 0.0f; } }
                        if (x == 1) { for (int i = 48; i < 64; i += 4) { colorArray[i] = 0.0f; colorArray[i + 1] = 1.0f; colorArray[i + 2] = 0.0f; } }
                        if (y == 1) { for (int i = 64; i < 80; i += 4) { colorArray[i] = 1.0f; colorArray[i + 1] = 1.0f; colorArray[i + 2] = 1.0f; } }
                        if (y == -1) { for (int i = 80; i < 96; i += 4) { colorArray[i] = 1.0f; colorArray[i + 1] = 1.0f; colorArray[i + 2] = 0.0f; } }

                        uint colorBuffer = Gl.GenBuffer();
                        Gl.BindBuffer(GLEnum.ArrayBuffer, colorBuffer);
                        Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
                        Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
                        Gl.EnableVertexAttribArray(1);

                        Gl.BindBuffer(GLEnum.ElementArrayBuffer, indicesBuffer);
                        Gl.BindVertexArray(0);

                        modelMatrixok[kockaIndex] = Matrix4X4.CreateTranslation(x * 1.05f, y * 1.05f, z * 1.05f);
                        forgoSzeletenVan[kockaIndex] = (y == 1);
                        kockaIndex++;
                    }
                }
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (ImGuiNET.ImGui.GetIO().WantCaptureKeyboard) return;

            switch (key)
            {
                case Key.Up: camera.MoveForward(); break;
                case Key.Down: camera.MoveBackward(); break;
                case Key.Left: camera.StrafeLeft(); break;
                case Key.Right: camera.StrafeRight(); break;
                case Key.Q: case Key.PageUp: camera.MoveUp(); break;
                case Key.E: case Key.PageDown: camera.MoveDown(); break;
                case Key.A: camera.TurnLeft(); break;
                case Key.D: camera.TurnRight(); break;
                case Key.W: camera.PitchUp(); break;
                case Key.S: camera.PitchDown(); break;
                case Key.Space: cubeArrangementModel.RotateSliceForward(); break;
                case Key.Backspace: cubeArrangementModel.RotateSliceBackward(); break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            cubeArrangementModel.AdvanceTime(deltaTime);
            imGuiController.Update((float)deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            int viewLoc = Gl.GetUniformLocation(program, "uView");
            Gl.UniformMatrix4(viewLoc, 1, false, (float*)&viewMatrix);

            var projMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 4), 800f / 800f, 0.1f, 100f);
            int projLoc = Gl.GetUniformLocation(program, "uProjection");
            Gl.UniformMatrix4(projLoc, 1, false, (float*)&projMatrix);

            Gl.Uniform3(Gl.GetUniformLocation(program, "lightColor"), lightColor.X, lightColor.Y, lightColor.Z);
            Gl.Uniform3(Gl.GetUniformLocation(program, "lightPos"), lightPosX, lightPosY, lightPosZ);
            Gl.Uniform3(Gl.GetUniformLocation(program, "viewPos"), camera.Position.X, camera.Position.Y, camera.Position.Z);

            int modelLoc = Gl.GetUniformLocation(program, "uModel");
            int sliceRotLoc = Gl.GetUniformLocation(program, "uSliceRotation");
            int normMatLoc = Gl.GetUniformLocation(program, "uNormalMatrix");

            var identityMatrix = Matrix4X4<float>.Identity;
            var sliceRotationMatrix = Matrix4X4.CreateRotationY((float)cubeArrangementModel.CurrentSliceAngle);

            for (int i = 0; i < 27; i++)
            {
                var matrix = modelMatrixok[i];
                Matrix4X4<float> worldMatrix;

                if (forgoSzeletenVan[i])
                {
                    Gl.UniformMatrix4(sliceRotLoc, 1, false, (float*)&sliceRotationMatrix);
                    worldMatrix = matrix * sliceRotationMatrix; 
                }
                else
                {
                    Gl.UniformMatrix4(sliceRotLoc, 1, false, (float*)&identityMatrix);
                    worldMatrix = matrix;
                }

                Gl.UniformMatrix4(modelLoc, 1, false, (float*)&matrix);

                var matNoTrans = new Matrix4X4<float>(worldMatrix.Row1, worldMatrix.Row2, worldMatrix.Row3, worldMatrix.Row4);
                matNoTrans.M41 = 0; matNoTrans.M42 = 0; matNoTrans.M43 = 0; matNoTrans.M44 = 1;

                Matrix4X4.Invert(matNoTrans, out var modelInv);
                Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInv));
                Gl.UniformMatrix3(normMatLoc, 1, false, (float*)&normalMatrix);

                Gl.BindVertexArray(vaos[i]);
                Gl.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, null);
            }

            ImGuiNET.ImGui.Begin("Rubik Kocka Beallitasok", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize);

            ImGuiNET.ImGui.SliderFloat3("Megvilagitas szine (R,G,B)", ref lightColor, 0.0f, 1.0f);

            ImGuiNET.ImGui.Separator();

            ImGuiNET.ImGui.Text("Fenyforras Pozicioja:");
            ImGuiNET.ImGui.InputFloat("X", ref lightPosX);
            ImGuiNET.ImGui.InputFloat("Y", ref lightPosY);
            ImGuiNET.ImGui.InputFloat("Z", ref lightPosZ);

            ImGuiNET.ImGui.Separator();

            if (ImGuiNET.ImGui.Button("Szelet Forgatasa Elore (+90 fok)"))
            {
                cubeArrangementModel.RotateSliceForward();
            }
            if (ImGuiNET.ImGui.Button("Szelet Forgatasa Hatra (-90 fok)"))
            {
                cubeArrangementModel.RotateSliceBackward();
            }

            ImGuiNET.ImGui.End();
            imGuiController.Render();
        }

        private static void GraphicWindow_Closing()
        {
            imGuiController?.Dispose();
            Gl.DeleteProgram(program);
        }
    }
}