﻿using CanvasRendering.Helpers;
using CanvasRendering.Shaders;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Contracts.Controls;

public unsafe class BaseControl
{
    private readonly GL _gl;
    private uint width;
    private uint height;

    public float Left { get; set; }

    public float Top { get; set; }

    public uint Width
    {
        get => width;
        set
        {
            if (width != value)
            {
                width = value;
                UpdateLayout();
            }
        }
    }

    public uint Height
    {
        get => height;
        set
        {
            if (height != value)
            {
                height = value;
                UpdateLayout();
            }
        }
    }

    public Matrix3X2<float> Transform { get; set; } = Matrix3X2<float>.Identity;

    public Vector2D<float> TransformOrigin { get; set; } = new(0.0f, 0.0f);

    public ICanvas Canvas { get; private set; }

    public bool IsUpdateLayout { get; private set; }

    public bool IsDirtyArea { get; private set; }

    public BaseControl(GL gl)
    {
        _gl = gl;
    }

    protected void UpdateLayout()
    {
        if (Width != 0 && Height != 0)
        {
            IsUpdateLayout = true;
            IsDirtyArea = true;
        }
    }

    protected void UpdateRender()
    {
        if (Width != 0 && Height != 0 && Canvas != null)
        {
            IsDirtyArea = true;
        }
    }

    /// <summary>
    /// 开始渲染
    /// </summary>
    public void StartRender()
    {
        if (IsDirtyArea)
        {
            if (IsUpdateLayout)
            {
                if (Canvas == null)
                {
                    Canvas = new SkiaCanvas(_gl, new Vector2D<uint>(Width, Height));
                }
                else
                {
                    Canvas.ResetSize(new Vector2D<uint>(Width, Height));
                }

                IsUpdateLayout = false;
            }

            Canvas.Begin();
            {
                Canvas.Clear();
                OnRender();
            }
            Canvas.End();

            IsDirtyArea = false;
        }
    }

    /// <summary>
    /// 绘制画板
    /// </summary>
    /// <param name="clip">裁剪</param>
    public void DrawOnWindow(ShaderProgram textureProgram, Rectangle<int>? clip = null)
    {
        if (Canvas is not SkiaCanvas canvas)
        {
            return;
        }

        if (clip != null)
        {
            // gl 左下角为原点，所以需要转换一下。
            _gl.Enable(EnableCap.ScissorTest);
            _gl.Scissor(clip.Value.Origin.X, CanvasDraw.Height - clip.Value.Max.Y, (uint)clip.Value.Size.X, (uint)clip.Value.Size.Y);
        }

        uint positionAttrib = (uint)textureProgram.GetAttrib(DefaultVertex.PositionAttrib);
        uint texCoordAttrib = (uint)textureProgram.GetAttrib(DefaultVertex.TexCoordAttrib);

        _gl.EnableVertexAttribArray(positionAttrib);
        _gl.EnableVertexAttribArray(texCoordAttrib);

        textureProgram.Enable();

        GetMatrix(out Matrix4X4<float> transform, out Matrix4X4<float> view, out Matrix4X4<float> projection);

        _gl.UniformMatrix4(textureProgram.GetUniform(DefaultVertex.TransformUniform), 1, false, (float*)&transform);
        _gl.UniformMatrix4(textureProgram.GetUniform(DefaultVertex.ViewUniform), 1, false, (float*)&view);
        _gl.UniformMatrix4(textureProgram.GetUniform(DefaultVertex.ProjectionUniform), 1, false, (float*)&projection);

        _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
        _gl.VertexAttribPointer(texCoordAttrib, 2, GLEnum.Float, false, 0, null);

        _gl.ActiveTexture(GLEnum.Texture0);
        _gl.BindTexture(GLEnum.Texture2D, canvas.Texture.TextureId);
        _gl.Uniform1(textureProgram.GetUniform(TextureFragment.TexUniform), 0);

        _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        textureProgram.Disable();

        _gl.DisableVertexAttribArray(positionAttrib);
        _gl.DisableVertexAttribArray(texCoordAttrib);

        _gl.Disable(EnableCap.ScissorTest);
    }

    protected virtual void OnRender()
    {
    }

    private void GetMatrix(out Matrix4X4<float> transform, out Matrix4X4<float> view, out Matrix4X4<float> projection)
    {
        transform = new Matrix4X4<float>(Transform)
                    * Matrix4X4.CreateTranslation(new Vector3D<float>(Left, Top, 0.0f))
                    * Matrix4X4.CreateScale(new Vector3D<float>(2.0f / CanvasDraw.Width, -(2.0f / CanvasDraw.Height), 0.0f), new Vector3D<float>(0, 0, 0.0f))
                    * Matrix4X4.CreateTranslation(new Vector3D<float>(-1.0f, 1.0f, 0.0f));

        view = CanvasDraw.Camera.View;

        projection = CanvasDraw.Camera.Projection;
    }
}