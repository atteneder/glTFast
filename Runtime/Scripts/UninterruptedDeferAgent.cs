namespace GLTFast {

    public class UninterruptedDeferAgent : IDeferAgent
    {
        public void Reset() {}

        public bool ShouldDefer() {
            return false;
        }
    }
}
