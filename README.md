# Procedural Infinite Terrain Generator

![Unity](https://img.shields.io/badge/Unity-2021%2B-blue.svg)
![C#](https://img.shields.io/badge/C%23-Programming-orange.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)

## Overview

The **Procedural Infinite Terrain Generator** is a Unity project that dynamically generates an infinite terrain using Perlin noise and object pooling techniques. The terrain seamlessly extends in all directions as the player moves, creating an immersive open-world experience.

## Features

✅ **Procedural Generation** – Uses Perlin noise to create natural-looking terrain.

✅ **Infinite Scrolling Terrain** – New terrain chunks generate dynamically as the player moves.

✅ **Level of Detail (LOD) Optimization** – Reduces complexity based on distance.

✅ **Multithreading Support** – Improves performance by generating terrain on separate threads.

✅ **Customizable Terrain Settings** – Adjust height maps, scale, and frequency.

✅ **Deterministic Terrain** – Chunks are deterministic: If you move far enough and then come back, the original chunk will remain unchanged.

## How It Works

### Terrain Generation
- The terrain is divided into chunks.
- Each chunk is generated using Perlin noise for realistic elevation.
- As the player moves, new chunks are spawned while distant chunks are removed or reused.

### Performance Optimization
- **Object Pooling:** Prevents frequent memory allocation by reusing terrain chunks.
- **Multithreading:** Terrain generation runs on a separate thread to prevent frame drops.
- **LOD System:** Reduces detail on distant terrain for better performance.

## Contributions
Contributions are welcome! Feel free to submit a pull request or report issues.

## Roadmap
- 🌟 Add rivers and lakes
- 🌟 Poisson Disc Sampling for procedural vegetation and rocks
- 🌟 Add support for different noise patterns

## Credits
Credits to Sebastian Lague and his tutorials for this project.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact
📧 **Email:** fernandoedelstein99@gmail.com
