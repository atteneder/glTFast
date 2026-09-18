// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Addons
{
    /// <summary>
    /// Base class for <see cref="IDataInstanceApplierFactory"/> implementations that own a piece of
    /// data produced during the conversion phase and create <see cref="IInstanceApplier"/> instances
    /// targeting a specific <see cref="IInstantiator"/> implementation.
    /// </summary>
    /// <typeparam name="TData">Type of the data produced during conversion and applied to instances.</typeparam>
    /// <typeparam name="TInstantiator">Concrete <see cref="IInstantiator"/> type the created appliers
    /// require.</typeparam>
    abstract class DataInstanceApplierFactory<TData, TInstantiator> : IDataInstanceApplierFactory
        where TInstantiator : IInstantiator
    {
        /// <summary>
        /// The data produced during the conversion phase that the created appliers will apply to
        /// instantiated scenes.
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// Creates a new factory wrapping the given conversion-phase data.
        /// </summary>
        /// <param name="data">Data to be applied to instantiated scenes.</param>
        protected DataInstanceApplierFactory(TData data)
        {
            Data = data;
        }

        /// <summary>
        /// Creates an <see cref="IInstanceApplier"/> for the given instantiator if it is of the
        /// expected concrete type <typeparamref name="TInstantiator"/>.
        /// </summary>
        /// <param name="instantiator">Instantiator that produced the scene instance.</param>
        /// <returns>An applier bound to <paramref name="instantiator"/>, or <see langword="null"/>
        /// if the instantiator is not of type <typeparamref name="TInstantiator"/>.</returns>
        public IInstanceApplier CreateInstanceApplier(IInstantiator instantiator)
        {
            if (instantiator is TInstantiator typedInstantiator)
            {
                return CreateInstanceApplier(typedInstantiator);
            }

            return null;
        }

        /// <summary>
        /// Creates an <see cref="IInstanceApplier"/> bound to the given typed instantiator.
        /// </summary>
        /// <param name="instantiator">Instantiator that produced the scene instance.</param>
        /// <returns>An applier that can apply <see cref="Data"/> to the instance.</returns>
        protected abstract IInstanceApplier CreateInstanceApplier(TInstantiator instantiator);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources held by this factory and, when <paramref name="disposing"/> is
        /// <see langword="true"/>, by <see cref="Data"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> when invoked from <see cref="Dispose()"/>;
        /// <see langword="false"/> when invoked from a finalizer.</param>
        protected abstract void Dispose(bool disposing);
    }

    /// <summary>
    /// Base class for <see cref="IDataInstanceApplierFactory"/> implementations that own a piece of
    /// data produced during the conversion phase and create <see cref="IInstanceApplier"/> instances
    /// for any <see cref="IInstantiator"/>.
    /// </summary>
    /// <typeparam name="TData">Type of the data produced during conversion and applied to instances.</typeparam>
    abstract class DataInstanceApplierFactory<TData> : IDataInstanceApplierFactory
    {
        /// <summary>
        /// The data produced during the conversion phase that the created appliers will apply to
        /// instantiated scenes.
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// Creates a new factory wrapping the given conversion-phase data.
        /// </summary>
        /// <param name="data">Data to be applied to instantiated scenes.</param>
        protected DataInstanceApplierFactory(TData data)
        {
            Data = data;
        }

        /// <summary>
        /// Creates an <see cref="IInstanceApplier"/> bound to the given instantiator.
        /// </summary>
        /// <param name="instantiator">Instantiator that produced the scene instance.</param>
        /// <returns>An applier that can apply <see cref="Data"/> to the instance, or
        /// <see langword="null"/> if the instantiator is not supported.</returns>
        public abstract IInstanceApplier CreateInstanceApplier(IInstantiator instantiator);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources held by this factory and, when <paramref name="disposing"/> is
        /// <see langword="true"/>, by <see cref="Data"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> when invoked from <see cref="Dispose()"/>;
        /// <see langword="false"/> when invoked from a finalizer.</param>
        protected abstract void Dispose(bool disposing);
    }
}
