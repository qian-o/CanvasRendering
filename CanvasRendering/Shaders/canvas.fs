#version 300 es
precision highp float;

in vec2 texCoord;
out vec4 fragColor;

// Solid color
uniform int useSolidColor;
uniform vec4 solidColor;

// Gradient Color
uniform int useGradientColor;
uniform vec4 gradientColors[10];
uniform float gradientRatios[10];
uniform float gradientAngle;
uniform int gradientCount;

void main()
{
   if (useSolidColor != 0)
   {
       fragColor = solidColor;
   }
   else if (useGradientColor != 0)
   {
        vec2 center = texCoord - 0.5;
        float angle = atan(center.y, center.x) + radians(gradientAngle);
        float radius = length(center) * 2.0;

        vec4 color = vec4(1.0, 0.0, 0.0, 1.0);
        float totalRatio = 0.0;

        for (int i = 0; i < gradientCount; i++)
        {
            if (gradientRatios[i] == 0.0)
            {
                continue;
            }
            totalRatio += gradientRatios[i];
            if (angle <= radians(360.0 * totalRatio))
            {
                float ratio = (angle - radians(360.0 * (totalRatio - gradientRatios[i]))) / radians(360.0 * gradientRatios[i]);
                color = mix(color, gradientColors[i], ratio);
                break;
            }
        }

        fragColor = color;
   }
   else
   {
       fragColor = vec4(1.0, 0.0, 0.0, 1.0);
   }
}