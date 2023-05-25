// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast.Loading
{

    /// <summary>
    /// Used for wrapping <see cref="Task{IDownload}"/>
    /// </summary>
    abstract class TextureDownloadBase
    {
        public IDownload Download { get; protected set; }

        /// <summary>
        /// Executes the texture loading process and assigns the result to
        /// <see cref="Download"/>.
        /// </summary>
        /// <returns></returns>
        public abstract Task Load();
    }

    /// <summary>
    /// Used for wrapping <see cref="Task{IDownload}"/>
    /// </summary>
    class TextureDownload<T> : TextureDownloadBase where T : IDownload
    {
        Task<T> m_Task;

        public TextureDownload(Task<T> task)
        {
            m_Task = task;
        }

        public override async Task Load()
        {
            Download = await m_Task;
        }
    }
}
