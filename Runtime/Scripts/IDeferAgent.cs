namespace GLTFast {

    /// <summary>
    /// An IDeferAgent can be used to interrupting the glTF loading procedure
    /// at certain points. This decision is always a trade-off between minimum
    /// loading time and a stable frame rate.
    /// </summary>
    public interface IDeferAgent {
        /// <summary>
        /// This will be called by GltFast at various points in the procedure.
        /// </summary>
        /// <returns>True if the remaining work of the loading procedure should
        /// be deferred to the next frame/Update loop invocation. False if
        /// work can continue.</returns>
        bool ShouldDefer();
    }
}