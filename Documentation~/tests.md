# Tests

> [!CAUTION]
> To do meaningful development you have to [switch to the Unity fork](./UpgradeGuides#transition-to-unity-gltfast), as this repository/branch does not contain the tools, test and projects required for development. See [Download Sources](./sources.md#download-sources) to learn where to obtain said fork.

*glTFast* comes with a set of unit and integration tests that ensure the software functions without unexpected errors and certify the correctness of the results. Those tests and the assets they require can be found in the tests package under `Packages/com.unity.cloud.gltfast.tests`.

Once your project is correctly setup (by either opening one of the provided [test projects](test-project-setup.md#test-projects) or following the steps to [setup your own project](test-project-setup.md#setup-a-custom-project)) you'll be able to run all tests via the *Test Runner* window (under *Window* → *General* → *Test Runner*; see [Test Framework documentation][UTFRunTests] for details about running tests).

> [!NOTE]
> Passing all tests is a requirement for getting a [contribution](contributing.md) accepted.

On an unmodified copy of *glTFast* all tests should pass. Once you start introducing changes it's recommended to periodically check that the tests still pass.

If a test doesn't succeed you'll have to investigate if it revealed a flaw in you modification or the tests itself needs to be adjusted.

## Adding Tests

If you add new code to *glTFast* you should also add tests that certify correctness of the new code.

For more information about creating and running tests, consult the documentation of the [Unity&reg; Test Framework][UTF]

## Test Assets

Some tests consume glTF&trade; files or other kinds of test input data. Those test assets can be found in the hidden directory `Packages/com.unity.cloud.gltfast.tests/Assets~`.

When tests are run in Editor playmode, the test assets are loaded from that location directly.

When tests are run in a player build, `Unity.Cloud.Gltfast.Editor.Tests.PreprocessBuild` copies all test assets into a directory named `gltfast` within [StreamingAssets][StreamingAssets], effectively packing the assets with the build and ensuring the files are accessible when the tests are run.

> [!CAUTION]
> If you ran the tests in a production project, you need to remove the test folders in StreamingAssets to ensure they don't end up in a production build.

## Performance Tests

*glTFast* comes with a set of procedurally generated glTFs and tests dedicated to measure and track performance metrics. In order to run the performance tests the [Performance Testing Package for Unity Test Framework][UTFPerformance] has to be installed, otherwise they won't show up.

The glTFs required for the tests have to be generated prior to running the tests by clicking *Tools* → *glTFast* → *Create performance test glTFs* from the main menu.

> [!TIP]
> The performance test can be included or excluded from test runs via the *Performance* test category they've been assigned to.

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][unity].

*Khronos&reg;* is a registered trademark and [glTF&trade;][gltf] is a trademark of [The Khronos Group Inc][khronos].

[gltf]: https://www.khronos.org/gltf
[khronos]: https://www.khronos.org
[StreamingAssets]: https://docs.unity3d.com/Manual/StreamingAssets.html
[unity]: https://unity.com
[UTF]: https://docs.unity3d.com/Packages/com.unity.test-framework@latest/
[UTFPerformance]: https://docs.unity3d.com/Packages/com.unity.test-framework.performance@latest/
[UTFRunTests]: https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/workflow-run-test.html
