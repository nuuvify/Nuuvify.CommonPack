# GitHub Copilot Instructions

## Project Specifications

- The current project is using the latest C# for .NET 8.0 and Visual Studio as the main IDE.
- Ensure all generated code and tests are compatible with these technologies.
- Ensure all generated code and tests follows Editorconfig rules at .editorconfig file located at project`s root path.
- Write tests for all new and modified code, in case the test already existis suggest the necessary modification to keep it up to date.

## Commit Message Generation

- The commit message must follow the Conventional Commits 2.0 specification.
- Structure example:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

- Examples for each commit type under Conventional Commits and SemVer 2.0:
- **feat**: A new feature
  ```
  feat(auth): add OAuth2 login

  - Implement OAuth2 login flow
  - Add unit tests for OAuth2 login
  - Update documentation with OAuth2 login instructions
  ```
- **fix**: A bug fix
  ```
  fix(api): correct user authentication issue

  - Fix token validation logic
  - Add integration tests for token validation
  ```
- **docs**: Documentation only changes
  ```
  docs(readme): update installation instructions

  - Add steps for setting up the project locally
  - Update prerequisites section
  ```
- **style**: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc.)
  ```
  style: format code according to .editorconfig

  - Apply consistent formatting across all files
  - Remove unnecessary white-space
  ```
- **refactor**: A code change that neither fixes a bug nor adds a feature
  ```
  refactor(database): optimize query performance

  - Refactor SQL queries for better performance
  - Update related unit tests
  ```
- **perf**: A code change that improves performance
  ```
  perf(cache): improve caching mechanism

  - Implement LRU cache for frequently accessed data
  - Add benchmarks for cache performance
  ```
- **test**: Adding missing tests or correcting existing tests
  ```
  test: add unit tests for user service

  - Add unit tests for user creation and deletion
  - Update mock data for user service tests
  ```
- **build**: Changes that affect the build system or external dependencies (example scopes: gulp, broccoli, npm)
  ```
  build: update build configuration

  - Upgrade to Webpack 5
  - Update build scripts for compatibility
  ```
- **ci**: Changes to our CI configuration files and scripts (example scopes: Travis, Circle, BrowserStack, SauceLabs)
  ```
  ci: update CI pipeline for faster builds

  - Parallelize test execution
  - Add caching for dependencies
  ```
- **chore**: Other changes that don't modify src or test files
  ```
  chore: update project dependencies

  - Bump version of lodash to 4.17.21
  - Remove unused dependencies
  ```
- **revert**: Reverts a previous commit
  ```
  revert: revert "feat(auth): add OAuth2 login"

  - This reverts commit abc1234
  ```

## Code Review Instructions

When reviewing code, ensure high code quality and security by following these guidelines:

- **Code Quality**:
  - Ensure the code adheres to the Editorconfig rules (stored in `.editorconfig` file located at project`s root path).
  - Check for consistent formatting and style.
  - Verify that there is no duplicated or redundant code.
  - Ensure that the code is easy to read and maintain.
  - Follow naming conventions for variables, methods, and classes.
  - Implement proper error handling and logging practices.
- **Security**:
  - Check for potential security vulnerabilities.
  - Ensure proper authentication and authorization mechanisms are in place.
  - Validate input and sanitize output to prevent injection attacks.
  - Use HTTPS for secure communication.
  - Securely store sensitive data, such as passwords and API keys.
  - Conduct regular security audits and code reviews.
- **Functionality**:
  - Verify that the code does what it is supposed to do.
  - Ensure that all new features are covered by tests.
  - Check that the documentation is updated and clear.

## Test Generation Instructions

When generating tests, use xUnit, Moc, Bogus and shared context between Tests to ensure comprehensive test coverage:

- **Test Coverage**:  
  - Cover edge cases and error scenarios in tests.
  - Ensure that all code paths are tested.
  - Use test-driven development (TDD) to write tests before implementing code.
  - Use [Theory] only for integration tests with InlineData when you need to test the same functionality with different parameters.
  - Use [Trait] to group tests by category, for example, [Trait("Category", "Unit")].
    - Use Unit for unit tests and Integration for integration tests.
- **Unit Tests**:
  - Mock dependencies where necessary to isolate the unit under test.
  - Ensure tests are deterministic and do not rely on external state.
  - Use only one method to validade all object preperties individually.
  - Use xUnit class fixtures when new test file is an entity or model reused in the workspace.
    - Make sure to create 2 different files, one for Faker and another for the test class.
      - The faker file must contain 2 public static methods with Bogus faker Generate method, one to generate a single new instance and the other to generate a collection receiving a count parameter.
    - Use shared context between faker and tests files to avoid code duplication.
- **Integration Tests**:
  - The test project must use `Microsoft.NET.Sdk.Web` SDK to be able to support `CustomWebApplicationFactory`.
  - The test project must contain a `CustomWebApplicationFactory` file to create a new instance of the web application for integration tests.
  - Test the interaction between components or services.
  - Use memory database or sandbox mock calls for integration tests.
    - When creating a new integration test file, make sure to create a new class fixture to create a new instance of the database context.
    - When test file is for a controller or a servive that call external services via http, make sure to use HttpTest from Flurl to mock the external service calls.
  - Ensure that integration tests are isolated and do not affect other tests.
  - Use shared context between tests to avoid code duplication.

## Code Generation Instructions

When generating code follow these best practices:

- Ensure the code follows the project's Editorconfig rules.
- Avoid using deprecated or unsafe APIs.
- Follow best practices for code readability and maintainability.
- Ensure the code is well-documented with comments where necessary.
