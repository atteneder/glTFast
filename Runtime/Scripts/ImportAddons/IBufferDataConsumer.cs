// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Gltfast.Addons
{
    /// <summary>
    /// Consumes a glTF asset's buffer data during import, once every buffer is loaded and decoded
    /// and before the import converts that data into Unity resources.
    /// To use this, implement the interface in an <see cref="ImportAddonInstance"/> and inject that instance.
    /// </summary>
    /// <remarks>
    /// Add-ons implementing this are invoked in unspecified order and potentially concurrently, so
    /// an implementation must not assume that it is the only one running, or that it runs before or
    /// after any other add-on.
    /// </remarks>
    /// <seealso cref="ImportAddonRegistry"/>
    /// <seealso cref="GltfImport.AddImportAddonInstance"/>
    public interface IBufferDataConsumer
    {
        /// <summary>
        /// Called once all of the glTF asset's buffers are available and decoded, before their data
        /// is converted into Unity resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Called on the main thread and has to return on it. In between it may schedule C# jobs
        /// or start threads of its own. Use <paramref name="bufferData"/> itself from the main
        /// thread only; the containers it provides may be read from those jobs and threads.
        /// </para>
        /// <para>
        /// The glTF asset is read-only here. An implementation must not modify
        /// <see cref="Objects.Root"/>, any glTF object or any buffer data. It is free to allocate,
        /// decode and create resources of its own, which it then owns.
        /// </para>
        /// <para>
        /// Returning false aborts the import. Whether the remaining add-ons are invoked, or run to
        /// completion, is unspecified. Report the reason through the import's
        /// <see cref="Logging.ICodeLogger"/>; the return value only signals success.
        /// </para>
        /// </remarks>
        /// <param name="bufferData">Read access to the asset's buffer data. It is disposed by the
        /// import right after this call returns. To keep reading beyond it, lease your own
        /// via <see cref="GltfImport.LeaseBufferData"/> and dispose that when done.</param>
        /// <param name="cancellationToken">Can be used to abort the loading procedure.</param>
        /// <returns>False if the loading process has to be aborted due to a critical error. True otherwise.</returns>
        Task<bool> ConsumeBufferDataAsync(IGltfBufferData bufferData, CancellationToken cancellationToken);
    }
}
