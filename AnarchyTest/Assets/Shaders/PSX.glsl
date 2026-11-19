// The first PlayStation's picture: drawn small (240 rows) and shown in big square pixels, with only 32 levels of each
// colour (15-bit colour) and the console's own 4x4 dither pattern smoothing the steps between them, fading into
// darkness with distance as its fog did. Pairs with the scene's Retro settings (vertex snapping, bent textures, crisp
// texels), which do the rest of the look as things are drawn.

property float Lines = 240 [120, 480];          // How many rows of pixels fill the screen's height
property float ColourBits = 5 [3, 8];           // Bits for each of red, green and blue: 5 is the PlayStation's
property float Dither = 1 [0, 2];               // How strongly the dither pattern smooths between colour levels
property color FogColour = #000000;             // What things fade into with distance
property float FogStart = 6 [0, 100];           // Metres away where the fog begins
property float FogEnd = 22 [1, 200];            // Metres away where only fog is left
property float Contrast = 1.08 [0.5, 2];        // Pushes colours apart a little, as old TVs did
property float Saturation = 1.1 [0, 2];         // How strong colours are
property float Fear = 0 [0, 1];                 // Interference while the white figure is about: the game sets it
property float Blackout = 0 [0, 1];             // How far the picture goes to black: the game blinks it

// A number from 0 to 1 that looks random, the same each time for the same place.
float scramble(vec2 p)
{
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

// The PlayStation added these to each 8-bit colour before dropping it to 5 bits, by where the pixel is.
const float PATTERN[16] = float[](-4.0, 0.0, -3.0, 1.0,
                                   2.0, -2.0, 3.0, -1.0,
                                  -3.0, 1.0, -4.0, 0.0,
                                   3.0, -1.0, 2.0, -2.0);

vec4 effect(vec2 uv)
{
    // Whole screen pixels per PlayStation pixel, so they're all the same size, and the one this is in.
    float size = max(1.0, floor(screenSize.y / Lines + 0.5));
    vec2 cell = floor(uv * screenSize / size);

    // Fear tears the picture: bands of rows jump sideways for a moment, more of them the more afraid.
    float tick = floor(time * 18.0);
    if (Fear > 0.0 && scramble(vec2(floor(cell.y / 3.0), tick)) > 1.0 - Fear * 0.35)
        cell.x += floor((scramble(vec2(cell.y, tick + 7.0)) - 0.5) * Fear * 24.0);

    vec2 centre = (cell + 0.5) * size / screenSize;
    vec3 colour = picture(centre).rgb;
    // And red slips out of line with the rest.
    colour.r = mix(colour.r, picture(centre + vec2(Fear * 2.0 * size / screenSize.x, 0.0)).r, Fear);

    // Fog: straight from the start to the end, as the console's depth cueing was.
    float fog = clamp((distanceAt(centre) - FogStart) / max(FogEnd - FogStart, 0.01), 0.0, 1.0);
    colour = mix(colour, FogColour.rgb, fog);

    float grey = dot(colour, vec3(0.299, 0.587, 0.114));
    colour = mix(vec3(grey), colour, Saturation);
    colour = (colour - 0.5) * Contrast + 0.5;

    // Drained towards a sick, reddish grey, with snow over it.
    colour = mix(colour, vec3(grey * 1.1, grey * 0.8, grey * 0.8), Fear * 0.5);
    colour += (scramble(cell + tick) - 0.5) * Fear * 0.15;
    colour *= 1.0 - Blackout;

    // As the console did it: the pattern's nudge added to the 8-bit colour, then the low bits dropped. With 5 bits,
    // each level is 8 apart, and the nudges are up to half of one.
    float levels = exp2(floor(ColourBits));
    float step = 256.0 / levels;
    ivec2 p = ivec2(mod(cell, 4.0));
    vec3 eight = colour * 255.0 + PATTERN[p.y * 4 + p.x] * Dither * step / 8.0;
    colour = floor(clamp(eight, 0.0, 255.0) / step) / (levels - 1.0);

    return vec4(colour, 1.0);
}
