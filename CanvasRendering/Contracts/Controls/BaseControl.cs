using CanvasRendering.Helpers;
using CanvasRendering.Shaders;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Contracts.Controls;

public unsafe class BaseControl
{
    private readonly GL _gl;

    private float left;
    private float top;
    private uint width;
    private uint height;
    private Matrix4X4<float> transform = Matrix4X4<float>.Identity;
    private Vector3D<float> transformOrigin = new(0.0f, 0.0f, 0.0f);

    public float Left { get => left; set { left = value; UpdateLayout(); } }

    public float Top { get => top; set { top = value; UpdateLayout(); } }

    public uint Width { get => width; set { width = value; UpdateLayout(); } }

    public uint Height { get => height; set { height = value; UpdateLayout(); } }

    public Matrix4X4<float> Transform { get => transform; set { transform = value; UpdateLayout(); } }

    public Vector3D<float> TransformOrigin { get => transformOrigin; set { transformOrigin = value; UpdateLayout(); } }

    public ICanvas Canvas { get; private set; }

    public bool IsDirtyArea { get; private set; }

    public BaseControl(GL gl)
    {
        _gl = gl;
    }

    protected void UpdateLayout()
    {
        if (Width != 0 && Height != 0)
        {
            Canvas?.Dispose();
            Canvas = new SkiaCanvas(_gl, new Vector2D<uint>(Width, Height));
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

        uint positionAttrib = (uint)textureProgram.GetAttribLocation(DefaultVertex.PositionAttrib);
        uint texCoordAttrib = (uint)textureProgram.GetAttribLocation(DefaultVertex.TexCoordAttrib);

        _gl.EnableVertexAttribArray(positionAttrib);
        _gl.EnableVertexAttribArray(texCoordAttrib);

        textureProgram.Enable();

        GetMatrix(out Matrix4X4<float> transform, out Matrix4X4<float> view, out Matrix4X4<float> perspective);

        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.TransformUniform), 1, false, (float*)&transform);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.ViewUniform), 1, false, (float*)&view);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.PerspectiveUniform), 1, false, (float*)&perspective);

        canvas.UpdateVertexBuffer(new Rectangle<float>((CanvasDraw.Width - Width) / 2.0f, (CanvasDraw.Height - Height) / 2.0f, Width, Height));
        canvas.UpdateTexCoordBuffer();

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

        _gl.Disable(EnableCap.ScissorTest);
    }

    protected virtual void OnRender()
    {
    }

    private void GetMatrix(out Matrix4X4<float> transform, out Matrix4X4<float> view, out Matrix4X4<float> perspective)
    {
        float max = new float[] { Width, Height }.Max();

        transform = Transform * Matrix4X4.CreateScale(1.0f, 1.0f, max / 2.0f / 1000000.0f) * Matrix4X4.CreateTranslation(new Vector3D<float>(Left - (CanvasDraw.Width - Width) / 2.0f, Top - (CanvasDraw.Height - Height) / 2.0f, 0.0f));

        view = Matrix4X4.CreateLookAt(new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector3D<float>(0.0f, 0.0f, 0.0f), new Vector3D<float>(0.0f, 1.0f, 0.0f));

        perspective = Matrix4X4.CreatePerspectiveOffCenter(0.0f, CanvasDraw.Width, CanvasDraw.Height, 0.0f, 1.0f, 100.0f);
    }
}