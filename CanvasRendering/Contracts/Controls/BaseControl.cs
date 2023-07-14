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
    private Matrix4x4 transform = Matrix4x4.Identity;
    private Vector3 transformOrigin = new(0.0f, 0.0f, 0.0f);

    public float Left { get => left; set { left = value; UpdateLayout(); } }

    public float Top { get => top; set { top = value; UpdateLayout(); } }

    public uint Width { get => width; set { width = value; UpdateLayout(); } }

    public uint Height { get => height; set { height = value; UpdateLayout(); } }

    public Matrix4x4 Transform { get => transform; set { transform = value; UpdateLayout(); } }

    public Vector3 TransformOrigin { get => transformOrigin; set { transformOrigin = value; UpdateLayout(); } }

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
    /// <param name="windowWidth">窗体宽度</param>
    /// <param name="windowHeight">窗体高度</param>
    /// <param name="textureProgram">纹理着色器程序</param>
    public void DrawOnWindow(ShaderProgram textureProgram)
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

        GetMatrix(out Matrix4x4 orthographic, out Matrix4x4 begin, out Matrix4x4 transform, out Matrix4x4 view, out Matrix4x4 perspective, out Matrix4x4 end);

        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.OrthographicUniform), 1, false, (float*)&orthographic);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.BeginUniform), 1, false, (float*)&begin);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.TransformUniform), 1, false, (float*)&transform);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.ViewUniform), 1, false, (float*)&view);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.PerspectiveUniform), 1, false, (float*)&perspective);
        _gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.EndUniform), 1, false, (float*)&end);

        canvas.UpdateVertexBuffer(new Rectangle<float>(Left, Top, Width, Height));
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
    }

    protected virtual void OnRender()
    {
    }

    private void GetMatrix(out Matrix4x4 orthographic, out Matrix4x4 begin, out Matrix4x4 transform, out Matrix4x4 view, out Matrix4x4 perspective, out Matrix4x4 end)
    {
        Vector2 centerPoint = new(Left + (Width / 2.0f), Top + (Height / 2.0f));
        centerPoint = Vector2.Transform(centerPoint, CanvasDraw.Orthographic);

        Vector3 originPoint = new(Left + (TransformOrigin.X * Width), Top + (TransformOrigin.Y * Height), TransformOrigin.Z);

        orthographic = CanvasDraw.Orthographic;

        begin = Matrix4x4.CreateTranslation(-centerPoint.X, -centerPoint.Y, 0.0f);

        transform = SetMatrixOrigin(Transform, originPoint);

        view = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));

        perspective = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, 1.0f, 1.0f, 100.0f);

        end = Matrix4x4.CreateTranslation(centerPoint.X, centerPoint.Y, 0.0f);
    }

    private static Matrix4x4 SetMatrixOrigin(Matrix4x4 matrix, Vector3 origin)
    {
        matrix.M41 = origin.X - (matrix.M11 * origin.X) - (matrix.M21 * origin.Y);
        matrix.M42 = origin.Y - (matrix.M12 * origin.X) - (matrix.M22 * origin.Y);
        matrix.M43 = origin.Z - (matrix.M13 * origin.X) - (matrix.M23 * origin.Y);

        return matrix;
    }
}