using Nuuvify.CommonPack.Extensions.Implementation;

var jsonMessage = "{\"sucesso\": true, \"data\": \"test\"}";
var result = jsonMessage.GetReturnMessageWithoutRn();
var expected = "{\"sucesso\":true, \"data\": \"test\"}";

Console.WriteLine($"Input:    '{jsonMessage}'");
Console.WriteLine($"Output:   '{result}'");
Console.WriteLine($"Expected: '{expected}'");
Console.WriteLine($"Match?    {result == expected}");

// Debug the method behavior
var initSuccess = jsonMessage.IndexOf("\"sucesso\":", 0, StringComparison.InvariantCultureIgnoreCase);
var intEnd = initSuccess + 10;
Console.WriteLine($"\nDebug:");
Console.WriteLine($"initSuccess: {initSuccess}");
Console.WriteLine($"intEnd: {intEnd}");
Console.WriteLine($"Substring(0, {intEnd}): '{jsonMessage.Substring(0, intEnd)}'");
Console.WriteLine($"After Replace(' ', ''): '{jsonMessage.Substring(0, intEnd).Replace(" ", "")}'");
Console.WriteLine($"Remaining: '{jsonMessage.Substring(intEnd)}'");
