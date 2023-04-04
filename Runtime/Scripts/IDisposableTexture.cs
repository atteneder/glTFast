using System;
using UnityEngine;

namespace GLTFast
{
    public interface IDisposableTexture : IDisposable
    {
        Texture2D Texture { get; }
    }
}