namespace GLTFast {

    public class UninterruptedDeferAgent : IDeferAgent
    {
        public bool ShouldDefer() {
            return false;
        }
    }
}
