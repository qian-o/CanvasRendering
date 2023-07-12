using CanvasRendering.Helpers;
using CanvasRendering.Shaders;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Numerics;

namespace CanvasRendering.Contracts.Controls;

public unsafe class BaseControl
{
    private readonly GL _gl;

    private float left;
    private float top;
    private uint width;
    private uint height;
    private Matrix4x4 layoutTransform = Matrix4x4.Identity;
    private Matrix3x2 renderTransform = Matrix3x2.Identity;

    public float Left { get => left; set { left = value; UpdateLayout(); } }

    public float Top { get => top; set { top = value; UpdateLayout(); } }

    public uint Width { get => width; set { width = value; UpdateLayout(); } }

    public uint Height { get => height; set { height = value; UpdateLayout(); } }

    public Matrix4x4 LayoutTransform { get => layoutTransform; set { layoutTransform = value; UpdateLayout(); } }

    public Matrix3x2 RenderTransform { get => renderTransform; set { renderTransform = value; UpdateLayout(); } }

    public ICanvas Canvas { get; private set; }

    public bool IsDirtyArea { get; private set; }

    public BaseControl(GL gl)
    {
        _gl = gl;
    }

    private void UpdateLayout()
    {
        if (Width != 0 && Height != 0)
        {
            Canvas?.Dispose();
            Canvas = new SkiaCanvas(_gl, new Vector2D<uint>(Width, Height));
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
            Canvas.Begin();
            Canvas.Clear();
            OnRender();
            Canvas.End();

            IsDirtyArea = false;
        }
    }

    /// <summary>
    /// 绘制画板
    /// </summary>
    /// <param name="windowWidth">窗体宽度</param>
    /// <param name="windowHeight">窗体高度</param>
    /// <param name="textureProgram">纹理着色器程序</param>
    public void DrawOnWindow(int windowWidth, int windowHeight, ShaderProgram textureProgram)
    {
        if (Canvas is not SkiaCanvas canvas)
        {
            return;
        }

        uint positionAttrib = (uint)textureProgram.GetAttribLocation(DefaultVertex.PositionAttrib);
        uint texCoordAttrib = (uint)textureProgram.GetAttribLocation(DefaultVertex.TexCoordAttrib);

        _gl.EnableVertexAttribArray(positionAttrib);
        _gl.EnableVertexAttribArray(texCoordAttrib);

        textureProgram.Enable();

        Matrix4x4 orthographic = Matrix4x4.CreateOrthographicOffCenter(0.0f, windowWidth, windowHeight, 0.0f, 0.0f, 1.0f);
        Matrix4x4 model = LayoutTransform;
        Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f));
        Matrix4x4 perspective = Matrix4x4.CreatePerspectiveFieldOfView((float)(90.0f * Math.PI / 180.0), 1.0f, 1.0f, 100.0f);

        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.OrthographicUniform), 1, false, (float*)&orthographic);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.ModelUniform), 1, false, (float*)&model);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.ViewUniform), 1, false, (float*)&view);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.PerspectiveUniform), 1, false, (float*)&perspective);

        canvas.UpdateVertexBuffer(new Rectangle<float>(Left, Top, Width, Height));
        canvas.UpdateTexCoordBuffer(RenderTransform);

        _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
        _gl.VertexAttribPointer(texCoordAttrib, 2, GLEnum.Float, false, 0, null);

        _gl.ActiveTexture(GLEnum.Texture0);
        _gl.BindTexture(GLEnum.Texture2D, canvas.Framebuffer.DrawTexture);
        _gl.Uniform1(textureProgram.GetUniformLocation(TextureFragment.TexUniform), 0);

        _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        textureProgram.Disable();

        _gl.DisableVertexAttribArray(positionAttrib);
        _gl.DisableVertexAttribArray(texCoordAttrib);
    }

    protected virtual void OnRender()
    {
    }
}