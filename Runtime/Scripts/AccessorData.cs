using System.Runtime.InteropServices;

namespace GLTFast
{
    enum AccessorUsage {
        Unknown,
        Ignore,
        Index,
        Position,
        Normal,
        Tangent,
        UV,
        Color
    }

    abstract class AccessorDataBase {
        public abstract void Unpin();
    }

    class AccessorData<T> : AccessorDataBase {
        public T[] data;
        public GCHandle gcHandle;

        public override void Unpin() {
            gcHandle.Free();
        }
    }
}
