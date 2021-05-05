using UnityEngine;

namespace GLTFast {

    public interface ITextureAssigner {
        void Assign(Texture texture);
        void Error();
    }

    public class TextureAssigner : ITextureAssigner {

        Material material;
        int propertyId;

        public TextureAssigner(Material material, int propertyId) {
            this.material = material;
            this.propertyId = propertyId;
        }

        public void Assign(Texture texture) {
            material.SetTexture(propertyId, texture);
        }

        public void Error() {
            // TODO: Report!
            Debug.LogError("Assigning Texture failed");
        }
    }
}
