// One-bit dithering: the picture in big square pixels, each either dark or light, with a fixed pattern of dots
// standing in for the greys between (a 2x2 ordered dither, so five tones: none, a quarter, a checkerboard, three
// quarters, all).

property float Lines = 360;             // How many rows of big pixels fill the screen's height
property float Brightness = 1 [0, 4];   // Scales the picture's brightness before it's dithered
property float Contrast = 1 [0, 4];     // Pushes greys towards dark or light
property bool Smooth = false;           // Averages each big pixel's area instead of taking its middle
property color Dark = #000000;          // The colour of pixels that are off
property color Light = #FFFFFF;         // The colour of pixels that are on

// Where each of the 2x2 cells turns on, darkest first.
float threshold(vec2 cell)
{
    vec2 p = mod(cell, 2.0);
    float order = p.y < 0.5 ? (p.x < 0.5 ? 0.0 : 2.0) : (p.x < 0.5 ? 3.0 : 1.0);
    return (order + 0.5) / 4.0;
}

vec4 effect(vec2 uv)
{
    // Whole screen pixels per big pixel, so they're all the same size.
    float size = max(1.0, floor(screenSize.y / Lines + 0.5));
    vec2 cell = floor(uv * screenSize / size);

    vec3 colour;
    if (Smooth)
    {
        colour = vec3(0.0);
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++)
                colour += picture((cell * size + (vec2(x, y) + 0.5) * size * 0.5) / screenSize).rgb;
        colour *= 0.25;
    }
    else
    {
        colour = picture((cell + 0.5) * size / screenSize).rgb;
    }

    float tone = dot(colour, vec3(0.299, 0.587, 0.114));
    tone = clamp((tone * Brightness - 0.5) * Contrast + 0.5, 0.0, 1.0);
    return tone > threshold(cell) ? Light : Dark;
}
