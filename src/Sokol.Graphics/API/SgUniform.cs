using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Sokol.sokol_gfx;

namespace Sokol
{
    public sealed class SgUniform
    {
        private readonly sg_shader_stage _shaderStage;
        
        internal readonly int Size;

        public SgShaderStage ShaderStage { get; }
        public SgShaderUniformType Type { get; }
        public string Name { get; }

        public SgUniform(SgShaderStage shaderStage, SgShaderUniformType type, string name = null)
        {
            _shaderStage = shaderStage switch
            {
                SgShaderStage.Fragment => sg_shader_stage.SG_SHADERSTAGE_FS,
                SgShaderStage.Vertex => sg_shader_stage.SG_SHADERSTAGE_VS,
                _ => throw new ArgumentOutOfRangeException(nameof(shaderStage))
            };
            
            ShaderStage = shaderStage;

            Size = type switch
            {
                SgShaderUniformType.Float => Unsafe.SizeOf<float>(),
                SgShaderUniformType.Float2 => Unsafe.SizeOf<Vector2>(),
                SgShaderUniformType.Float3 => Unsafe.SizeOf<Vector3>(),
                SgShaderUniformType.Float4 => Unsafe.SizeOf<Vector4>(),
                SgShaderUniformType.Matrix4X4 => Unsafe.SizeOf<Matrix4x4>(),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            Type = type;
            Name = name;
        }

        public unsafe void Apply<T>(ref T value) where T : unmanaged
        {
            var size = Unsafe.SizeOf<T>();

            if (size != Size)
            {
                throw new InvalidOperationException();
            }
            
            var pointer = Unsafe.AsPointer(ref value);
            sg_apply_uniforms(_shaderStage, 0, pointer, size);
        }
    }
}