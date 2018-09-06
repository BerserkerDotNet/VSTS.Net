# Contributing to Vsts.Net

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

When contributing to this repository, please first discuss the change you wish to make via issue,
email, or any other method with the owner of this repository before making a change.

## Pull Request Process

### Before submitting

* To avoid merge conflicts, make sure your branch is rebased on the `master` branch of this repository.
* Many code changes will require new tests,
  so make sure you've added a new test if existing tests do not effectively test the code changed.
  
### Pull request - Submission

**Always create a pull request to the `master` branch of this repository**.

* It's recommended to avoid a PR with too many changes.
  A large PR not only stretches the review time, but also makes it much harder to spot issues.
  In such case, it's better to split the PR to multiple smaller ones.
  For large features, try to approach it in an incremental way, so that each PR won't be too big.
* Add a meaningful title of the PR describing what change you want to check in.
  Don't simply put: "Fix issue #5".
  Also don't directly use the issue title as the PR title.
  An issue title is to briefly describe what is wrong, while a PR title is to briefly describe what is changed.
  A better example is: "Add ExecuteFlatQuery method to VstsClient", with "Fix #5" in the PR's body.
* When you create a pull request,
  including a summary about your changes in the PR description.
  The description is used to create change logs,
  so try to have the first sentence explain the benefit to end users.
  If the changes are related to an existing GitHub issue,
  please reference the issue in PR description (e.g. ```Fix #11```).
