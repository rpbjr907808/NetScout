# Contributing to NetScout

First off, thank you for considering contributing to NetScout! It's people like you that make NetScout a great tool.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples**
- **Describe the behavior you observed and what you expected**
- **Include your environment details** (OS, .NET version, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- **Use a clear and descriptive title**
- **Provide a detailed description of the suggested enhancement**
- **Explain why this enhancement would be useful**
- **List any similar tools that have this feature**

### Pull Requests

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. Ensure your code follows the existing style
4. Make sure your code builds without warnings
5. Update the README.md if needed
6. Issue the pull request!

## Development Setup

1. Clone your fork of the repo
```bash
git clone https://github.com/your-username/NetScout.git
```

2. Install .NET 8.0 SDK if not already installed

3. Restore dependencies
```bash
dotnet restore
```

4. Build the project
```bash
dotnet build
```

5. Run the application
```bash
dotnet run
```

## Code Style Guidelines

- Use meaningful variable and method names
- Add XML documentation comments for public methods
- Follow C# naming conventions (PascalCase for classes/methods, camelCase for variables)
- Keep methods focused and single-purpose
- Use async/await for I/O operations

## Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Reference issues and pull requests when relevant
- First line should be 50 characters or less
- Provide detailed description in the body if needed

Example:
```
Add UDP port scanning capability

- Implement UDP scanning in NetworkScanner class
- Add UDP port enumeration to OpenPort model
- Update README with UDP scanning documentation

Fixes #42
```

## Testing

Before submitting a pull request:

1. Test your changes thoroughly
2. Run the application and verify it works as expected
3. Test on different network configurations if possible
4. Ensure no build warnings or errors

## Questions?

Feel free to open an issue with the tag "question" if you have any questions about contributing!

Thank you for contributing to NetScout! 🎉
