using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.AutoHistory;

/// <summary>
/// Represents the entity change history.
/// </summary>
public class AutoHistory
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    /// <value>The id.</value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the source row id.
    /// </summary>
    /// <value>The source row id.</value>
    public string RowId { get; set; }

    /// <summary>
    /// Use any value, but generally used to map every change coming from a request.
    /// </summary>
    /// <value></value>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    /// <value>The name of the table.</value>
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the json about Before changing.
    /// </summary>
    /// <value>The json about Before changing.</value>
    public string Before { get; set; }
    /// <summary>
    /// Gets or sets the json about After changing.
    /// </summary>
    /// <value>The json about After changing.</value>
    public string After { get; set; }

    /// <summary>
    /// Gets or sets the change kind.
    /// </summary>
    /// <value>The change kind.</value>
    public EntityState Kind { get; set; }

    /// <summary>
    /// Gets or sets the create time.
    /// </summary>
    /// <value>The create time.</value>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// ToSave parameter of the SaveChanges method
    /// </summary>
    /// <value>true</value>
    public string PersistInDatabase { get; set; }

    /// <summary>
    /// Valid user in the application context, usernameContext parameter of EnsureAutoHistory method
    /// </summary>
    /// <value></value>
    public string Username { get; set; }

}

/// <summary>
/// This class exists so we can reference AutoHistory in the test project. The class name collides with the namespace there.
/// </summary>
internal class AutoHistoryTestHandle : AutoHistory
{

}
