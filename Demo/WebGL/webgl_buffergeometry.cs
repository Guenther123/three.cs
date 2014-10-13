﻿namespace Demo
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using Examples;



    using ThreeCs;
    using ThreeCs.Cameras;
    using ThreeCs.Core;
    using ThreeCs.Lights;
    using ThreeCs.Materials;
    using ThreeCs.Math;
    using ThreeCs.Objects;
    using ThreeCs.Renderers;
    using ThreeCs.Scenes;

    [Example("webgl_buffergeometry", ExampleCategory.WebGL, "BufferGeometry")]
    class webgl_buffergeometry : Example
    {
        private PerspectiveCamera camera;

        private Scene scene;

        private Mesh mesh;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        public override void Load(Control control)
        {
            base.Load(control);

            camera = new PerspectiveCamera(27, control.Width / (float)control.Height, 1, 3500);
            this.camera.Position.Z = 2750;

            scene = new Scene();
            scene.Fog = new Fog((Color)colorConvertor.ConvertFromString("#050505"), 2000, 3500);

            scene.Add(new AmbientLight((Color)colorConvertor.ConvertFromString("#444444")));

            var light1 = new DirectionalLight(Color.White, 0.5f);
            light1.Position = new Vector3(1, 1, 1);
            scene.Add(light1);

            var light2 = new DirectionalLight(Color.White, 1.5f);
            light2.Position = new Vector3(0, -1, 0);
            scene.Add(light2);

            //

            const int triangles = 160000;

            // break geometry into
            // chunks of 21,845 triangles (3 unique vertices per triangle)
            // for indices to fit into 16 bit integer number
            // floor(2^16 / 3) = 21845

            const int chunkSize = 21845;

            var indices = new ushort[triangles * 3];

            for (var i = 0; i < indices.Length; i++)
            {
                indices[i] = (ushort)(i % (3 * chunkSize));
            }

            var positions = new float[triangles * 3 * 3];
            var normals = new float[triangles * 3 * 3];
            var colors = new float[triangles * 3 * 3];

            var color = Color.White;

            const int n = 800; var n2 = n / 2;	// triangles spread in the cube
            const int d = 12; var d2 = d / 2;	// individual triangle size

            for (var i = 0; i < positions.Length; i += 9)
            {

                // positions

                var x = (float)(random.NextDouble() * n - n2);
                var y = (float)(random.NextDouble() * n - n2);
                var z = (float)(random.NextDouble() * n - n2);

                var ax = (float)(x + random.NextDouble() * d - d2);
                var ay = (float)(y + random.NextDouble() * d - d2);
                var az = (float)(z + random.NextDouble() * d - d2);

                var bx = (float)(x + random.NextDouble() * d - d2);
                var by = (float)(y + random.NextDouble() * d - d2);
                var bz = (float)(z + random.NextDouble() * d - d2);

                var cx = (float)(x + random.NextDouble() * d - d2);
                var cy = (float)(y + random.NextDouble() * d - d2);
                var cz = (float)(z + random.NextDouble() * d - d2);

                positions[i + 0] = ax;
                positions[i + 1] = ay;
                positions[i + 2] = az;

                positions[i + 3] = bx;
                positions[i + 4] = by;
                positions[i + 5] = bz;

                positions[i + 6] = cx;
                positions[i + 7] = cy;
                positions[i + 8] = cz;

                // flat face normals

                var pA = new Vector3(ax, ay, az);
                var pB = new Vector3(bx, by, bz);
                var pC = new Vector3(cx, cy, cz);

                var cb = new Vector3().SubtractVectors(pC, pB);
                var ab = new Vector3().SubtractVectors(pA, pB);
                cb.Cross(ab).Normalize();

                var nx = cb.X;
                var ny = cb.Y;
                var nz = cb.Z;

                normals[i + 0] = nx;
                normals[i + 1] = ny;
                normals[i + 2] = nz;

                normals[i + 3] = nx;
                normals[i + 4] = ny;
                normals[i + 5] = nz;

                normals[i + 6] = nx;
                normals[i + 7] = ny;
                normals[i + 8] = nz;

                // colors

                var vx = (x / n) + 0.5;
                var vy = (y / n) + 0.5;
                var vz = (z / n) + 0.5;

                color = Color.FromArgb(255, (int)(vx * 255), (int)(vy * 255), (int)(vz * 255));

                colors[i + 0] = color.R / 255.0f;
                colors[i + 1] = color.G / 255.0f;
                colors[i + 2] = color.B / 255.0f;

                colors[i + 3] = color.R / 255.0f;
                colors[i + 4] = color.G / 255.0f;
                colors[i + 5] = color.B / 255.0f;

                colors[i + 6] = color.R / 255.0f;
                colors[i + 7] = color.G / 255.0f;
                colors[i + 8] = color.B / 255.0f;

            }

            var geometry = new BufferGeometry();

            geometry.AddAttribute("index", new BufferAttribute<ushort>(indices, 1));
            geometry.AddAttribute("position", new BufferAttribute<float>(positions, 3));
            geometry.AddAttribute("normal", new BufferAttribute<float>(normals, 3));
            geometry.AddAttribute("color", new BufferAttribute<float>(colors, 3));

            const int offsets = triangles / chunkSize;

            for (var i = 0; i < offsets; i++)
            {
                var offset = new Offset
                {
                    Start = i * chunkSize * 3,
                    Index = i * chunkSize * 3,
                    Count = Math.Min(triangles - (i * chunkSize), chunkSize) * 3
                };

                geometry.offsets.Add(offset);

            }

            geometry.ComputeBoundingSphere();

            var material = new MeshPhongMaterial
            {
                color = (Color)colorConvertor.ConvertFromString("#aaaaaa"),
                ambient = (Color)colorConvertor.ConvertFromString("#aaaaaa"),
                specular = (Color)colorConvertor.ConvertFromString("#ffffff"),
                shininess = 250,
                side = Three.DoubleSide,
                vertexColors = new Color[Three.VertexColors]
            };

            this.mesh = new Mesh(geometry, material);
            scene.Add(mesh);

            renderer.SetClearColor(scene.Fog.color);

            renderer.gammaInput = true;
            renderer.gammaOutput = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        public override void Resize(Size clientSize)
        {
            Debug.Assert(null != this.camera);
            Debug.Assert(null != this.renderer);

            this.camera.Aspect = clientSize.Width / (float)clientSize.Height;
            this.camera.UpdateProjectionMatrix();

            this.renderer.size = clientSize;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Render()
        {
            Debug.Assert(null != this.mesh);
            Debug.Assert(null != this.renderer);
            
            var time = stopWatch.ElapsedMilliseconds;

            var ftime = time * 0.001f;

            this.mesh.Rotation.X = ftime * 0.25f;
            this.mesh.Rotation.Y = ftime * 0.5f;

            renderer.Render(scene, camera);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Unload()
        {
            this.scene.Dispose();
            this.camera.Dispose();

            base.Unload();
        }
    }
}
