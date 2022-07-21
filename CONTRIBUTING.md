# Contributing

Thank you for your interest in contributing to glTFast!

Here are our guidelines for contributing:

* [Code of Conduct](#coc)
* [Ways to Contribute](#ways)
  * [Issues and Bugs](#issue)
  * [Feature Requests](#feature)
  * [Improving Documentation](#docs)
* [Unity Contribution Agreement](#cla)
* [Pull Request Submission Guidelines](#submit-pr)


## <a name="coc"></a> Code of Conduct

Please help us keep glTFast open and inclusive. Read and follow our [Code of Conduct](CODE_OF_CONDUCT.md).

## <a name="ways"></a> Ways to Contribute

There are many ways in which you can contribute to glTFast.

### <a name="issue"></a> Issues and Bugs

If you find a bug in the source code, you can help us by submitting an issue to our
GitHub Repository. Even better, you can submit a Pull Request with a fix.

### <a name="feature"></a> Feature Requests

You can request a new feature by submitting an issue to our GitHub Repository.

If you would like to implement a new feature then consider what kind of change it is:

* `Major Changes` that you wish to contribute to the project should be discussed first with other developers. Submit your ideas as an issue.

* `Small Changes` can be directly submitted to the GitHub Repository
  as a Pull Request. See the section about [Pull Request Submission Guidelines](#submit-pr).

### <a name="docs"></a> Documentation

We accept changes and improvements to our documentation. Just submit a Pull Request with your proposed changes as described in the [Pull Request Submission Guidelines](#submit-pr).


## <a name="cla"></a> All contributions are subject to the [Unity Contribution Agreement(UCA)](https://unity3d.com/legal/licenses/Unity_Contribution_Agreement)

By making a pull request, you are confirming agreement to the terms and conditions of the UCA, including that your Contributions are your original creation and that you have complete right and authority to make your Contributions.

## <a name="submit-pr"></a> Pull Request Submission Guidelines

We use the [Gitflow Workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow) for the development of glTFast. This means development happens on the **unity branch** and Pull Requests should be submited to it.

### <a name="branch"></a> Branch Name Prefix
  - `bug/` Fixing a bug
  - `feature/` New feature implementation
  - `perf/` Performance improvement
  - `refactor/` A code change that neither fixes a bug nor adds a feature
  - `doc/` Added documentation
  - `test/` Added Unit Tests
  - `build/` Changes that affect the build system or external dependencies
  - `ci/` Changes to our CI configuration files and scripts
  - `style/` Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)

### Requirements
  - The branch name has the respective [prefix](#branch).
  - [Changelog](CHANGELOG.md) entry added under `Unreleased` section.
    - Explains the change in `Modified`, `Fixed`, `Added` sections.
    - For API change contains an example snippet and/or migration example.
    - If UI or rendering results applies, include screenshots.
    - FogBugz ticket attached, example `([case %number%](https://issuetracker.unity3d.com/issues/...))`.
    - FogBugz is marked as `Resolved` with `next` release version correctly set.
  - Tests added/changed, if applicable.
    - Functional tests.
    - Performance tests.
    - Integration tests.
  - All Tests passed.
  - Documentation was added for new/changed.
    - XmlDoc cross references are set correctly.
    - Added explanation how the API works.
    - Usage code examples added.
  - Coding Standards are respected.
  - Rebase the branch if possible.