# HxGLTF.MonoGame – GLTF Loader with Quick Rendering

**HxGLTF.MonoGame** is an extension to [HxGLTF](https://www.nuget.org/packages/H073.HxGLTF), providing a lightweight way to render loaded GLTF/GLB files using **MonoGame**.

> 💡 This is intended for **prototyping only**. The rendering system is minimal.  
> For production use, **implement your own renderer** using the loaded data.

---

## 🔌 What’s Included

- MonoGame-compatible classes like `GameModel`, `GameNode`, etc.
- A simple renderer: `PreImpGameModelRenderer`
- Optional precompiled shader
- Minimal boilerplate to visualize GLTFs

---

## 🚀 Quickstart (Example)

```csharp

// 1. Initialize the Renderer
var renderer = new PreImpGameModelRenderer(GraphicsDevice);
renderer.LoadPreCompiledEffect(Content); // Optional precompiled shader

// 2. Load the Model
var model = GameModel.From(GraphicsDevice, GLTFLoader.Load("model.glb"));

// 3. (Optional) Play Animation in Update
// model.Play(index);

// 4. Draw the Model in Draw() (with optional shader)
renderer.DrawModel(model, world, view, projection);
// renderer.DrawModel(model, world, view, projection, effect);

```

> 🧪 Use this for **testing** or **early-stage development**.

---

## ⚠ Limitations

- Not all GLTF features are supported by the sample renderer
- Indexed meshes may render incorrectly (e.g. Sketchfab models)
- Some models may need to be fixed in Blender
- Designed for **understanding** and **customization**, not full game-ready use

---

## 🖼 Visual Examples

### Basic Animation (Built-in Renderer)
| Single | Multiple |
|--------|----------|
| ![Idle](https://i.imgur.com/K2twCyw.gif) | ![Walk](https://i.imgur.com/Pw7S9Fr.gif) |

### Custom Renderer Examples
> Rendered manually using HxGLTF’s data

| UV Animations | Atlas Effect |
|---------------|--------------|
| ![Conveyor](https://i.imgur.com/Wswj4jU.gif) | ![Lights](https://i.imgur.com/OzDnRAW.gif) |

---

## 🧼 Fixing Broken Models

Sketchfab models or exported GLBs may have **bad mesh indices**.

✅ Fix:
- Open in **Blender**
- Export again as `.glb`

---

## 📦 Installation

```bash
dotnet add package H073.HxGLTF.MonoGame
```

Or in `.csproj`:

```xml
<PackageReference Include="H073.HxGLTF.MonoGame" Version="x.y.z" />
```

---

## 📚 Related

- [HxGLTF (Core Loader)](https://www.nuget.org/packages/H073.HxGLTF)  
  → Use it if you want full control over your rendering pipeline.

---

## 🤝 Contact

Discord: **sameplayer**
