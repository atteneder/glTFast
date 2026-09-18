// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Unity.Cloud.Gltfast.Addons
{
    class QueryableList<TMember> : List<TMember>
    {
        public delegate bool TryCreateResult<in TAddon, in TInput, TResult>
            (TAddon addon, TInput input, out TResult result);

        public T Get<T>() where T : ImportAddonInstance
        {
            foreach (var addon in this)
            {
                if (addon is T typedAddon)
                {
                    return typedAddon;
                }
            }

            return null;
        }

        public QueryableList<T> SubCollection<T>()
        {
            QueryableList<T> result = null;

            foreach (var addon in this)
            {
                if (addon is T typedAddon)
                {
                    result ??= new QueryableList<T>();
                    result.Add(typedAddon);
                }
            }

            return result;
        }

        public bool Any<T>(Func<T, bool> predicate)
        {
            foreach (var instance in this)
            {
                if (instance is T target && predicate(target))
                {
                    return true;
                }
            }

            return false;
        }

        public T First<T>()
        {
            foreach (var instance in this)
            {
                if (instance is T target)
                {
                    return target;
                }
            }

            return default;
        }

        public T First<T>(Func<T, bool> predicate)
        {
            foreach (var instance in this)
            {
                if (instance is T target && predicate(target))
                {
                    return target;
                }
            }

            return default;
        }

        public void ForEach<T>(Action<T> action)
        {
            foreach (var instance in this)
            {
                if (instance is T addon)
                {
                    action(addon);
                }
            }
        }

        public bool TryGet<TAddon, TInput, TResult>(
            TInput input,
            TryCreateResult<TAddon, TInput, TResult> action,
            out TResult result
        )
        {
            foreach (var instance in this)
            {
                if (instance is TAddon typedInstance && action(typedInstance, input, out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }

        public void ForEachTryGet<TInput, TResult>(
            IReadOnlyList<TInput> list,
            TryCreateResult<TMember, TInput, TResult> predicate,
            Action<TMember, int, TResult> resultAction
        )
        {
            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                foreach (var addon in this)
                {
                    if (predicate(addon, element, out var result))
                    {
                        resultAction(addon, i, result);
                        break;
                    }
                }
            }
        }
    }
}
