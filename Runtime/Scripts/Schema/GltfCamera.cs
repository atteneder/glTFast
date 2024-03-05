// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Schema
{

    /// <inheritdoc />
    [Serializable]
    public class Camera : CameraBase<CameraOrthographic, CameraPerspective> { }

    /// <inheritdoc />
    /// <typeparam name="TOrthographic">Orthographic camera type</typeparam>
    /// <typeparam name="TPerspective">Perspective camera type</typeparam>
    [Serializable]
    public abstract class CameraBase<TOrthographic, TPerspective> : CameraBase
        where TOrthographic : CameraOrthographic
        where TPerspective : CameraPerspective
    {
        /// <inheritdoc cref="Orthographic"/>
        public TOrthographic orthographic;

        /// <inheritdoc cref="Perspective"/>
        public TPerspective perspective;

        /// <inheritdoc />
        public override CameraOrthographic Orthographic => orthographic;

        /// <inheritdoc />
        public override CameraPerspective Perspective => perspective;
    }

    /// <summary>
    /// A camera’s projection
    /// </summary>
    [Serializable]
    public abstract class CameraBase : NamedObject
    {

        /// <summary>
        /// Camera projection type
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Orthogonal projection
            /// </summary>
            Orthographic,
            /// <summary>
            ///  Perspective projection
            /// </summary>
            Perspective
        }

        /// <inheritdoc cref="Type"/>
        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use GetCameraType and SetCameraType for access.")]
        public string type;

        Type? m_TypeEnum;

        /// <summary>
        /// <see cref="Type"/> typed and cached getter onto <see cref="type"/> string.
        /// </summary>
        /// <returns>Camera type, if it was retrieved correctly. <see cref="Type.Perspective"/> otherwise</returns>
        public Type GetCameraType()
        {
            if (m_TypeEnum.HasValue)
            {
                return m_TypeEnum.Value;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (Enum.TryParse<Type>(type, true, out var typeEnum))
            {
                m_TypeEnum = typeEnum;
                type = null;
                return m_TypeEnum.Value;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            if (Orthographic != null) m_TypeEnum = Type.Orthographic;
            if (Perspective != null) m_TypeEnum = Type.Perspective;
            return m_TypeEnum ?? Type.Perspective;
        }

        /// <summary>
        /// <see cref="Type"/> typed setter for <see cref="type"/> string.
        /// </summary>
        /// <param name="cameraType">Camera type</param>
        public void SetCameraType(Type cameraType)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            type = null;
#pragma warning restore CS0618 // Type or member is obsolete
            m_TypeEnum = cameraType;
        }

        /// <inheritdoc cref="CameraOrthographic"/>
        public abstract CameraOrthographic Orthographic { get; }

        /// <inheritdoc cref="CameraOrthographic"/>
        public abstract CameraPerspective Perspective { get; }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeName(writer);
            writer.AddProperty("type", m_TypeEnum.ToString().ToLowerInvariant());
            if (Perspective != null)
            {
                writer.AddProperty("perspective");
                Perspective.GltfSerialize(writer);
            }
            if (Orthographic != null)
            {
                writer.AddProperty("orthographic");
                Orthographic.GltfSerialize(writer);
            }
            writer.Close();
        }
    }

    /// <summary>
    /// An orthographic camera containing properties to create an orthographic projection matrix.
    /// </summary>
    [Serializable]
    public class CameraOrthographic
    {

        /// <summary>
        /// The floating-point horizontal magnification of the view. Must not be zero.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float xmag;

        /// <summary>
        /// The floating-point vertical magnification of the view. Must not be zero.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float ymag;

        /// <summary>
        /// The floating-point distance to the far clipping plane.
        /// <see cref="zfar"/> must be greater than <see cref="znear"/>.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float zfar;

        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float znear;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            // ReSharper disable StringLiteralTypo
            writer.AddProperty("xmag", xmag);
            writer.AddProperty("ymag", ymag);
            writer.AddProperty("zfar", zfar);
            writer.AddProperty("znear", znear);
            // ReSharper restore StringLiteralTypo
            writer.Close();
        }
    }

    /// <summary>
    /// A perspective camera containing properties to create a perspective projection matrix.
    /// </summary>
    [Serializable]
    public class CameraPerspective
    {

        /// <summary>
        /// The floating-point aspect ratio of the field of view.
        /// </summary>
        public float aspectRatio = -1;

        /// <summary>
        /// The floating-point vertical field of view in radians.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float yfov;

        /// <summary>
        /// The floating-point distance to the far clipping plane.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float zfar = -1f;

        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public float znear;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (aspectRatio > 0)
            {
                writer.AddProperty("aspectRatio", aspectRatio);
            }
            // ReSharper disable StringLiteralTypo
            writer.AddProperty("yfov", yfov);
            if (zfar < float.MaxValue)
            {
                writer.AddProperty("zfar", zfar);
            }
            writer.AddProperty("znear", znear);
            // ReSharper restore StringLiteralTypo
            writer.Close();
        }
    }
}
