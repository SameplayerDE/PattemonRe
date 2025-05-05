# HxGLTF - GLTF Loader for MonoGame

**HxGLTF** is a lightweight and focused library for loading **GLTF** and **GLB** files into MonoGame.

> 🧪 **Quick Start Included**  
> While HxGLTF is not intended to be a complete rendering engine, it provides a **basic predefined renderer** and an optional **precompiled shader** for quick prototyping.  
> This allows you to get models on screen quickly — ideal for testing or getting started.  
> However, this implementation is **not optimized**, **not feature-complete**, and **not meant for production use**.  
> As your project grows, you're encouraged to implement your **own rendering pipeline** using the structured data the library provides.

> ⚠️ **Rendering is Your Responsibility**  
> HxGLTF’s main goal is to load GLTF/GLB files into structured, usable C# objects.  
> You are expected to take over from there and build your own renderer based on your specific needs.


## Purpose

HxGLTF is designed to:
- Parse GLTF / GLB files
- Provide structured data: scenes, nodes, meshes, skins, textures, animations
- Integrate easily with MonoGame projects

It is **not designed to**:
- Fully render models out of the box
- Handle all rendering or shader needs
- Abstract away rendering complexity

## Features

- Load `.gltf` and `.glb` files
- Convert them into C# objects (`GameModel`, `GameNode`, etc.)
- Animation, skinning, and texture info included
- Works directly with MonoGame

---

## Quick Start (Optional Renderer)

For convenience, a **predefined renderer** is included:

- `PreImpGameModelRenderer`
- Includes an optional **precompiled shader**
- Can display models with basic lighting, skinning, and textures

⚠️ This renderer is **not comprehensive**. It does **not handle all models correctly**.  
Its only goal is to **get something on screen quickly**.

```csharp
var renderer = new PreImpGameModelRenderer(GraphicsDevice);
renderer.LoadPreCompiledEffect(Content); // Optional
var model = GameModel.From(GraphicsDevice, GLTFLoader.Load("model.glb"));
renderer.DrawModel(model, worldMatrix, viewMatrix, projectionMatrix);
```

### Renderer Limitations

- The **indexed mesh rendering issues** (e.g. broken triangles) are **only present in the built-in renderer**.
- The data is **correctly loaded** — if you write your own renderer, you are in full control.
- The built-in system is minimal and may not handle all edge cases.
- But it does show that the data works — and gets **something visual** running fast.

---

## Installation

```bash
dotnet add package H073.HxGLTF
```

Or in `.csproj`:

```xml
<PackageReference Include="H073.HxGLTF" Version="1.0.0" />
```

---

## Visuals
 
### 🧪 Default Renderer (Quick Start)

These animations were rendered using the included predefined renderer:

| Idle | Walk |
|------|------|
| ![Idle](https://i.imgur.com/K2twCyw.gif) | ![Walk](https://i.imgur.com/aoXY3gZ.gif) |

### 🧪 Custom Renderer
> 💡 The following visuals are **not auto-generated features** of the library —  
> they are shown to **demonstrate what’s possible** when you build your own renderer using HxGLTF's data.  
> Things like **UV animations** and **texture atlas effects** go beyond standard GLTF features —  
> but they’re entirely doable with the data you get from this library.  
> The goal here is to **inspire**, not to prescribe.
### 🌊 UV Animations (Custom Renderer)

| Conveyor Belt | Water Surface |
|---------------|---------------|
| ![Conveyor](https://i.imgur.com/Wswj4jU.gif) | ![Water](https://i.imgur.com/hQ0dDcH.gif) |

---

### ✨ Atlas Animation (Custom Renderer)

| Game Center Lights |
|--------------------|
| ![Lights](https://i.imgur.com/OzDnRAW.gif) |

---

## Indexed Mesh Fix (Sketchfab, etc.)

Some models — especially from **Sketchfab** — contain complex or broken mesh data that may fail in the sample renderer.

✅ **If you run into visual issues:**

- Open the model in **Blender**
- Re-export as `.glb`

This often fixes mesh indices and makes them work even in the simple renderer.

> ⚠️ Reminder: The issue is **only in the basic renderer**, not in the data loading itself.

---

## Understand the Format

To get the most out of HxGLTF, we recommend reading the [GLTF 2.0 Specification](https://github.com/KhronosGroup/glTF).

This will help you:
- Understand the structure of your models
- Build a proper rendering system
- Handle nodes, hierarchies, animations, skins, and materials correctly

---

## Contact

For feedback or help, reach out via Discord: **sameplayer**
