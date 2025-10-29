using System.Security;
using System.Text.RegularExpressions;

namespace System.IO.Abstractions;

public static class PathHelper
{
    /// <summary>
    /// Sanitizes and normalizes a file or directory path by cleaning up separators and handling special cases.
    /// </summary>
    /// <param name="rawPath">The raw path string to be sanitized and normalized.</param>
    /// <returns>A sanitized and normalized path string with proper directory separators for the current platform.</returns>
    /// <exception cref="ArgumentException">Thrown when the raw path is null, empty, or contains only whitespace characters.</exception>
    /// <remarks>
    /// This method performs the following operations:
    /// - Trims leading and trailing whitespace
    /// - Normalizes directory separators to the platform-specific separator
    /// - Removes empty path segments
    /// - Handles drive letters in Windows paths
    /// - Preserves UNC path prefixes (\\server\share or //server/share)
    /// - Combines path segments using the appropriate platform separator
    /// </remarks>
    public static string SanitizeAndNormalizePath(string rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            throw new ArgumentException("O caminho não pode ser nulo ou vazio.");

        var cleanedPath = rawPath.Trim()
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        var parts = cleanedPath.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        var resultPath = string.Empty;

        foreach (var part in parts)
        {
            if (Regex.IsMatch(part, @"^[a-zA-Z]:$") &&
                !string.IsNullOrEmpty(resultPath))
                continue;

            resultPath = Path.Combine(resultPath, part);
        }

        if (rawPath.StartsWith("\\", StringComparison.InvariantCultureIgnoreCase) ||
            rawPath.StartsWith("//", StringComparison.InvariantCultureIgnoreCase))
        {
            resultPath = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{resultPath.TrimStart(Path.DirectorySeparatorChar)}";
        }

        return resultPath;
    }

    public static bool PathExists(string path)
    {
        return Directory.Exists(path) || File.Exists(path);
    }

    /// <summary>
    /// Checks if the specified path has read and write access.
    /// This method works cross-platform by attempting to create and delete a test file.
    /// Works on both Windows and Unix-like systems with .NET Standard 2.1.
    /// </summary>
    /// <param name="path">The path to check for read/write access.</param>
    /// <returns>True if the path has read and write access, false otherwise.</returns>
    public static bool HasReadWriteAccess(string path)
    {
        if (!PathExists(path))
            return false;

        return TryReadWriteAccess(path);
    }

    /// <summary>
    /// Tests read/write access by attempting to create and delete a temporary file.
    /// This approach works on all platforms (.NET Standard 2.1 compatible).
    /// </summary>
    private static bool TryReadWriteAccess(string path)
    {
        try
        {
            // Tenta criar e deletar um arquivo temporário no diretório
            // Funciona tanto no Windows quanto no Linux/Unix
            var testFile = Path.Combine(path, ".access_test_" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException ||
                                   ex is DirectoryNotFoundException ||
                                   ex is IOException ||
                                   ex is SecurityException ||
                                   ex is ArgumentException ||
                                   ex is NotSupportedException)
        {
            // Captura exceções relacionadas a acesso negado, diretório não encontrado,
            // E/S, segurança, argumentos inválidos ou operações não suportadas
            // Retorna false pois não há permissão de leitura/escrita
            return false;
        }
    }
}
