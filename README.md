# CanvasRendering
这是使用 OpenGL ES 对 2D绘图进行的一个封装，使用 Skia 进行纹理绘制，绘制完成后交给 OpenGL 进行渲染。</br>
所有绘制函数都在 Canvas 中实现，性能上不敢有太多的保证，也做了类似脏区渲染的特性。</br>
GLES 与 Skia 各司其职，Skia 只负责绘制点线面，有关 Effect、Transform 交给 GLES 处理。</br>
当然，Skia也支持framebuffer的绑定，但我测试下来发现性能并不客观（在存在大量Canvas对象绘制时）。</br>
以下是 5000 个 Canvas 同时进行绘制的性能测试。</br>
Windows - NVIDIA GeForce RTX 3050 Laptop GPU
![image](https://github.com/qian-o/CanvasRendering/assets/84434846/594c6f30-bbff-4357-9418-507a38f0d355)
Linux - VirtualBox
![image](https://github.com/qian-o/CanvasRendering/assets/84434846/12b06010-25c3-40e1-9193-37006ced5ee7)
Android - 骁龙 8 Gen2（SM8550）
![76edad98c33e31092b0c0106fa41c529](https://github.com/qian-o/CanvasRendering/assets/84434846/239e8798-760f-408b-910f-5510edb5dfbb)

对于大量矩形来说，Win和Linux还算凑合，优化空间还是蛮大的。Android嘛。。。谁会没事在一个小屏幕上放那么多东西。🧐
