# Physical Light Units in glTF&trade;

Material emission and light intensities in glTF have physical units.

| Light type        | Unit                                  |
|-------------------| ------------------------------------- |
| Point/Spot        | [Candela (lumen per steradian)][cd]   |
| Directional       | [Lux (lumen per square meter)][lx]    |
| Emissive Material | [Nit (candela per square meter)][nit] |

But these units are only a part of the equation, since the perceived brightness of a light depends on the sensitivity of the viewer. In case of a physical camera, the sensitivity is controlled by its aperture, exposure time and film sensitivity (ISO). For example, a candle might light up an entire dark room, but is barely noticeable on a bright, sunny day. That's because the camera settings are adjusted to the overall brightness of the scene. This is called exposure control.

Unfortunately exposure control is not yet specified in glTF. Also, many render pipelines (e.g. Universal and Built-in Render Pipeline) do not integrate this concept. They can be thought of having a fixed exposure with a high sensitivity, so that a light with low, single-digit intensity value is enough to bring a scene to full illumination. The implication is that in those exposure-unaware render pipelines correct, physical light units have to be scaled down in order not to overexpose the scene.

An additional problem is, hardly any glTF viewer that lacks exposure control adjust glTF light values, so in practice many glTF creators are forced to use very low, physically implausible light intensity values within glTF assets to compensate the lack of exposure. The High Definition Render Pipeline supports exposure control and accepts values in glTF units in a physically correct manner. In this case the light intensity values have to get amplified to get a realistic result.

## Light Intensity

### Adjusting Values

*Unity glTFast* lets you multiply light intensities by an arbitrary factor (which defaults to 1.0) to make them fit your scene's lighting.

#### Physically Plausible Values in exposure-unaware Render Pipelines

TODO: Write guide how to get a correct light intensity scale factor

#### Physically Implausible Values in exposure-aware Render Pipelines

TODO: Write guide how to get a correct light intensity scale factor

### Reference Values

Here are some light intensity reference values. You may also consult the High Definition Render Pipeline's documentation about [Physical light units][hdrp-plu].

#### Illuminance of Directional Lights

| Surfaces illuminated by                        | Illuminance (lux) |
|------------------------------------------------|:-----------------:|
| Moonless, overcast night sky (starlight)       | 0.0001            |
| Moonless clear night sky with airglow          | 0.002             |
| Full moon on a clear night                     | 0.05–0.3          |
| Dark limit of civil twilight under a clear sky | 3.4               |
| Public areas with dark surroundings            | 20–50             |
| Family living room lights (Australia, 1998)    | 50                |
| Office building hallway/toilet lighting        | 80                |
| Very dark overcast day                         | 100               |
| Train station platforms                        | 150               |
| Office lighting                                | 320–500           |
| Sunrise or sunset on a clear day.              | 400               |
| Overcast day; typical TV studio lighting       | 1000              |
| Full daylight (not direct sun)                 | 10,000–25,000     |
| Direct sunlight                                | 32,000–100,000    |

Source: [Wikipedia][lux-illuminance]

#### Luminous Flux of Point and Spot Lights

| Source                                | Luminous flux (lumens)  |
|---------------------------------------|:-----------------------:|
| 37 mW "Superbright" white LED         | 0.20                    |
| 15 mW green laser (532 nm wavelength) | 8.4                     |
| 1 W high-output white LED             | 25–120                  |
| Kerosene lantern                      | 100                     |
| 40 W incandescent lamp at 230 volts   | 325                     |
| 7 W high-output white LED             | 450                     |
| 6 W COB filament LED lamp             | 600                     |
| 18 W fluorescent lamp                 | 1250                    |
| 100 W incandescent lamp               | 1750                    |
| 40 W fluorescent lamp                 | 2800                    |
| 35 W xenon bulb                       | 2200–3200               |
| 100 W fluorescent lamp                | 8000                    |
| 127 W low pressure sodium vapor lamp  | 25,000                  |
| 400 W metal-halide lamp               | 40,000                  |

Source: [Wikipedia][lum-flux]

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][khronos].

[cd]: https://en.wikipedia.org/wiki/Candela
[hdrp-plu]: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@13.1/manual/Physical-Light-Units.html
[khronos]: https://www.khronos.org
[lx]: https://en.wikipedia.org/wiki/Lux
[lux-illuminance]: https://en.wikipedia.org/wiki/Lux#Illuminance
[lum-flux]: https://en.wikipedia.org/wiki/Luminous_flux#Examples
[nit]: https://en.wikipedia.org/wiki/Candela_per_square_metre
[unity]: https://unity.com
