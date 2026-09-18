// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Application-specific data (<c>extras</c>) of a glTF JSON object.
    /// </summary>
    /// <remarks>
    /// The glTF specification allows <c>extras</c> to be any JSON value, not just an object. When it is
    /// an object, its properties are accessible through <see cref="IPropertyContainer"/>. Otherwise
    /// <see cref="Kind"/> reports the actual kind and <see cref="RawValue"/> provides the value.
    /// </remarks>
    /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#reference-extras"/>
    public class ExtrasContainer : AdditionalPropertyContainer
    {
        /// <summary>
        /// The <c>extras</c> value, if it is not a JSON object.
        /// </summary>
        /// <remarks>
        /// A default <see cref="JsonElement"/> reports <see cref="JsonValueKind.Undefined"/>, which
        /// serves as the "this is a regular JSON object" sentinel.
        /// <para>
        /// Serialization writes this value verbatim and ignores everything else, so anything that
        /// populates the object form has to reset it first. <see cref="Set{T}"/> and
        /// <see cref="Clear"/> do; a derived type that declares a (de-)serialized member, like
        /// <see cref="MeshExtras.TargetNames"/>, has to do the same in its setter.
        /// </para>
        /// </remarks>
        internal JsonElement RawValueElement { get; set; }

        /// <summary>
        /// The kind of JSON value this <c>extras</c> holds.
        /// </summary>
        /// <remarks>
        /// <see cref="ValueKind.Object"/> for the common case, where the properties are accessible via
        /// <see cref="AdditionalPropertyContainer.Count"/>,
        /// <see cref="AdditionalPropertyContainer.Keys"/>, the indexer and
        /// <see cref="AdditionalPropertyContainer.TryGetValue{T}"/>. Any other kind means the glTF
        /// carried a non-object <c>extras</c> value; read it via <see cref="RawValue"/>. In that case
        /// this container has no properties, so <c>Count</c> is 0 and <c>Keys</c> is empty.
        /// </remarks>
        [JsonIgnore]
        public ValueKind Kind => RawValueElement.ValueKind == JsonValueKind.Undefined
            ? ValueKind.Object
            : (ValueKind)RawValueElement.ValueKind;

        /// <summary>
        /// The <c>extras</c> value, for when <see cref="Kind"/> is not <see cref="ValueKind.Object"/>.
        /// </summary>
        /// <value>The value. Of kind <see cref="ValueKind.Undefined"/> if <c>extras</c> is an object.</value>
        [JsonIgnore]
        public Value RawValue => new(RawValueElement);

        /// <inheritdoc/>
        /// <remarks>Setting a property turns this <c>extras</c> into a JSON object, discarding a
        /// non-object <see cref="RawValue"/>.</remarks>
        public override void Set<T>(string key, T value)
        {
            RawValueElement = default;
            base.Set(key, value);
        }

        /// <inheritdoc/>
        /// <remarks>Also discards a non-object <see cref="RawValue"/>.</remarks>
        public override void Clear()
        {
            RawValueElement = default;
            base.Clear();
        }
    }
}
